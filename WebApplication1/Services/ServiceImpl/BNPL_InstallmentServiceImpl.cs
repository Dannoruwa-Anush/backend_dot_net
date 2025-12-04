using WebApplication1.DTOs.RequestDto.Payment;
using WebApplication1.DTOs.ResponseDto.BnplSnapshotPayingSimulation;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.DTOs.ResponseDto.Payment.Bnpl;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.UOW.IUOW;
using WebApplication1.Utils.Helpers;
using WebApplication1.Utils.Project_Enums;
using WebApplication1.Utils.SystemConstants;

namespace WebApplication1.Services.ServiceImpl
{
    public class BNPL_InstallmentServiceImpl : IBNPL_InstallmentService
    {
        private readonly IBNPL_InstallmentRepository _repository;
        private readonly IBNPL_PlanTypeRepository _bNPL_PlanTypeRepository;

        //logger: for auditing
        private readonly ILogger<BNPL_InstallmentServiceImpl> _logger;

        // Constructor
        public BNPL_InstallmentServiceImpl(IBNPL_InstallmentRepository repository, IBNPL_PlanTypeRepository bNPL_PlanTypeRepository, ILogger<BNPL_InstallmentServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _bNPL_PlanTypeRepository = bNPL_PlanTypeRepository;
            _logger = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<BNPL_Installment>> GetAllBNPL_InstallmentsAsync() =>
            await _repository.GetAllWithBnplDetailsAsync();

        public async Task<BNPL_Installment?> GetBNPL_InstallmentByIdAsync(int id) =>
            await _repository.GetWithBnplInDetailsByIdAsync(id);

        //Custom Query Operations
        public async Task<PaginationResultDto<BNPL_Installment>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? bnpl_Installment_StatusId = null, string? searchKey = null)
        {
            return await _repository.GetAllWithPaginationAsync(pageNumber, pageSize, bnpl_Installment_StatusId, searchKey);
        }

        public async Task<PaginationResultDto<BNPL_Installment>> GetAllWithPaginationByOrderIdAsync(int orderId, int pageNumber, int pageSize, int? bnpl_Installment_StatusId = null, string? searchKey = null)
        {
            return await _repository.GetAllWithPaginationByOrderIdAsync(orderId, pageNumber, pageSize, bnpl_Installment_StatusId, searchKey);
        }

        public async Task<IEnumerable<BNPL_Installment>> GetAllByPlanIdAsync(int planId) =>
            await _repository.GetAllByPlanIdAsync(planId);

        public async Task<List<BNPL_Installment>> GetAllUnsettledInstallmentByPlanIdAsync(int planId) =>
            await _repository.GetAllUnsettledInstallmentByPlanIdAsync(planId);

        //Shared Internal Operations Used by Multiple Repositories
        public async Task<List<BNPL_Installment>> BuildBnplInstallmentBulkAddRequestAsync(BNPL_PLAN plan)
        {
            if (plan == null)
                throw new ArgumentNullException(nameof(plan));

            // Ensure valid installment count
            if (plan.Bnpl_TotalInstallmentCount <= 0)
                throw new Exception("BNPL plan must have at least one installment.");

            // Load plan type to determine installment spacing
            var planType = await _bNPL_PlanTypeRepository.GetByIdAsync(plan.Bnpl_PlanTypeID);
            if (planType == null)
                throw new Exception($"BNPL Plan Type {plan.Bnpl_PlanTypeID} is invalid or missing.");

            int daysPerInstallment = planType.Bnpl_DurationDays;

            var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);
            var freeTrialDays = BnplSystemConstants.FreeTrialPeriodDays;

            var installmentCount = plan.Bnpl_TotalInstallmentCount;
            var installments = new List<BNPL_Installment>(installmentCount);

            // First due date
            var firstDueDate = now.AddDays(freeTrialDays);

            for (int i = 1; i <= installmentCount; i++)
            {
                installments.Add(new BNPL_Installment
                {
                    Bnpl_PlanID = plan.Bnpl_PlanID,
                    InstallmentNo = i,
                    Installment_BaseAmount = plan.Bnpl_AmountPerInstallment,
                    Installment_DueDate = firstDueDate.AddDays(daysPerInstallment * (i - 1)),
                    TotalDueAmount = plan.Bnpl_AmountPerInstallment,
                    Bnpl_Installment_Status = BNPL_Installment_StatusEnum.Pending
                });
            }

            if (installments.Any())
            {
                _logger.LogInformation("{NoInstallments} installments created for Bnpl planId={PlanId}", installments.Count, installments.First().Bnpl_PlanID);
            }
            else
            {
                _logger.LogWarning("No installments were created for Bnpl planId={PlanId}", plan.Bnpl_PlanID);
            }

            return installments;
        }

        //---- [Start : installment payment] -----
        //Main Driver Method : Installmet Payment
        public BnplInstallmentPaymentResultDto BuildBnplInstallmentSettlement(CustomerOrder existingOrder, BnplLatestSnapshotSettledResultDto latestSnapshotSettledResult)
        {
            var bnplPlan = existingOrder.BNPL_PLAN
                ?? throw new Exception("BNPL plan not found for the order.");

            var resultDto = new BnplInstallmentPaymentResultDto();

            decimal remainingArrears = latestSnapshotSettledResult.TotalPaidArrears;
            decimal remainingLateInterest = latestSnapshotSettledResult.TotalPaidLateInterest;
            decimal remainingBase = latestSnapshotSettledResult.TotalPaidCurrentInstallmentBase;
            decimal remainingOverpayment = latestSnapshotSettledResult.OverPaymentCarriedToNextInstallment;

            var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            // ----------------------------
            // Step 1: Process Past Installments (Arrears + Late Interest)
            // ----------------------------

            // Filter unpaid installments
            var unpaidOrPartiallyPaid = bnplPlan.BNPL_Installments
                .Where(i =>
                    i.Bnpl_Installment_Status != BNPL_Installment_StatusEnum.Paid_OnTime &&
                    i.Bnpl_Installment_Status != BNPL_Installment_StatusEnum.Paid_Late &&
                    i.Bnpl_Installment_Status != BNPL_Installment_StatusEnum.Refunded)
                .OrderBy(i => i.Installment_DueDate)
                .ThenBy(i => i.InstallmentNo)
                .ToList();

            ProcessArrearsAndLateFees(unpaidOrPartiallyPaid, ref remainingArrears, ref remainingLateInterest, resultDto, now);

            // ----------------------------
            // Step 2: Process Current Installment (Base + Overpayment) → nearest installment by due date
            // ----------------------------
            // Filter nearest installments
            var nearestInstallment = bnplPlan.BNPL_Installments
                .OrderBy(i => i.Installment_DueDate)
                .FirstOrDefault(i => i.Installment_DueDate >= now);

            if (nearestInstallment != null)
            {
                ProcessCurrentBaseAndOverpayment(nearestInstallment, ref remainingBase, remainingOverpayment, resultDto, now);
            }

            // ----------------------------
            // Step 3: Update BNPL plan and customer order
            // ----------------------------
            UpdateBnplPlanAndOrder(bnplPlan, existingOrder, now);

            return resultDto;
        }

        //Helper method to handle late installments (Overdue)
        private void ProcessArrearsAndLateFees(List<BNPL_Installment> installments, ref decimal remainingArrears, ref decimal remainingLateInterest, BnplInstallmentPaymentResultDto resultDto, DateTime today)
        {
            foreach (var inst in installments)
            {
                var breakdown = new BnplPerInstallmentPaymentBreakdownResultDto
                {
                    InstallmentId = inst.InstallmentID
                };

                // Apply arrears to base
                if (remainingArrears > 0)
                {
                    decimal arrearsDue = inst.Installment_BaseAmount - inst.AmountPaid_AgainstBase;
                    decimal apply = Math.Min(arrearsDue, remainingArrears);

                    inst.AmountPaid_AgainstBase += apply;
                    remainingArrears -= apply;
                    breakdown.AppliedToArrears = apply;
                }

                // Apply late interest
                if (remainingLateInterest > 0)
                {
                    decimal lateDue = inst.LateInterest - inst.AmountPaid_AgainstLateInterest;
                    decimal apply = Math.Min(lateDue, remainingLateInterest);

                    inst.AmountPaid_AgainstLateInterest += apply;
                    remainingLateInterest -= apply;
                    breakdown.AppliedToLateInterest = apply;
                }

                inst.TotalDueAmount = inst.Installment_BaseAmount + inst.LateInterest;
                inst.LastPaymentDate = today;
                inst.Bnpl_Installment_Status = DetermineInstallmentStatus(inst, today);

                resultDto.PerInstallmentBreakdown.Add(breakdown);
                resultDto.AppliedToArrears += breakdown.AppliedToArrears;
                resultDto.AppliedToLateInterest += breakdown.AppliedToLateInterest;
            }
        }

        //Helper method to handle current paymnets
        private void ProcessCurrentBaseAndOverpayment(BNPL_Installment currentInstallment, ref decimal remainingBase, decimal remainingOverpayment, BnplInstallmentPaymentResultDto resultDto, DateTime today)
        {
            var breakdown = new BnplPerInstallmentPaymentBreakdownResultDto
            {
                InstallmentId = currentInstallment.InstallmentID
            };

            // 1. Apply Base
            if (remainingBase > 0)
            {
                decimal baseDue = currentInstallment.Installment_BaseAmount - currentInstallment.AmountPaid_AgainstBase;
                decimal apply = Math.Min(baseDue, remainingBase);

                currentInstallment.AmountPaid_AgainstBase += apply;
                remainingBase -= apply;
                breakdown.AppliedToBase = apply;
            }

            // 2. Apply Overpayment
            if (remainingOverpayment > 0)
            {
                currentInstallment.OverpaymentCarriedToNextMonth += remainingOverpayment;
                breakdown.OverPayment = remainingOverpayment;
            }

            currentInstallment.TotalDueAmount = currentInstallment.Installment_BaseAmount + currentInstallment.LateInterest;
            currentInstallment.LastPaymentDate = today;
            currentInstallment.Bnpl_Installment_Status = DetermineInstallmentStatus(currentInstallment, today);

            resultDto.PerInstallmentBreakdown.Add(breakdown);
            resultDto.AppliedToBase += breakdown.AppliedToBase;
            resultDto.OverPayment += breakdown.OverPayment;
        }

        //Helper method to Update BNPL plan and customer order
        private void UpdateBnplPlanAndOrder(BNPL_PLAN bnplPlan, CustomerOrder existingOrder, DateTime now)
        {
            int remaining = bnplPlan.BNPL_Installments.Count(i => i.RemainingBalance > 0);
            bnplPlan.Bnpl_RemainingInstallmentCount = remaining;

            if (remaining == 0)
            {
                // Complete plan
                bnplPlan.Bnpl_NextDueDate = null;
                bnplPlan.Bnpl_Status = BnplStatusEnum.Completed;
                bnplPlan.CompletedAt = now;

                existingOrder.PaymentCompletedDate = now;
                existingOrder.OrderPaymentStatus = OrderPaymentStatusEnum.Fully_Paid;
            }
            else
            {
                var nextInst = bnplPlan.BNPL_Installments
                    .Where(i => i.RemainingBalance > 0)
                    .OrderBy(i => i.InstallmentNo)
                    .First();

                bnplPlan.Bnpl_Status = BnplStatusEnum.Active;
                bnplPlan.Bnpl_NextDueDate = nextInst.Installment_DueDate;
                existingOrder.OrderPaymentStatus = OrderPaymentStatusEnum.Partially_Paid;
            }
        }

        //Helper method to determine installment status
        private BNPL_Installment_StatusEnum DetermineInstallmentStatus(BNPL_Installment inst, DateTime today)
        {
            bool fullyPaid = inst.RemainingBalance <= 0;
            bool paidOnTime = inst.LastPaymentDate.HasValue && inst.LastPaymentDate.Value.Date <= inst.Installment_DueDate.Date;

            if (fullyPaid)
                return paidOnTime ? BNPL_Installment_StatusEnum.Paid_OnTime
                                  : BNPL_Installment_StatusEnum.Paid_Late;

            if (today > inst.Installment_DueDate.Date)
                return BNPL_Installment_StatusEnum.Overdue;

            if (inst.AmountPaid_AgainstBase > 0 || inst.AmountPaid_AgainstLateInterest > 0)
                return paidOnTime ? BNPL_Installment_StatusEnum.PartiallyPaid_OnTime
                                  : BNPL_Installment_StatusEnum.PartiallyPaid_Late;

            return BNPL_Installment_StatusEnum.Pending;
        }
        //---- [End : installment payment] -------
    }
}