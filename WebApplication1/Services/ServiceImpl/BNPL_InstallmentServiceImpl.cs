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
        public (BnplInstallmentPaymentResultDto Result, List<BNPL_Installment> UpdatedInstallments) BuildBnplInstallmentSettlementAsync(List<BNPL_Installment> installments, BnplLatestSnapshotSettledResultDto latestSnapshotSettledResult)
        {
            var updatedInstallments = new List<BNPL_Installment>();
            var resultDto = new BnplInstallmentPaymentResultDto();

            decimal remainingArrears = latestSnapshotSettledResult.TotalPaidArrears;
            decimal remainingLateInterest = latestSnapshotSettledResult.TotalPaidLateInterest;
            decimal remainingBase = latestSnapshotSettledResult.TotalPaidCurrentInstallmentBase;
            decimal remainingOverpayment = latestSnapshotSettledResult.OverPaymentCarriedToNextInstallment;

            var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            //FILTER: Only unpaid installments + due today or earlier
            var installmentsToProcess = installments
                .Where(i =>
                    i.Bnpl_Installment_Status != BNPL_Installment_StatusEnum.Paid_OnTime &&
                    i.Bnpl_Installment_Status != BNPL_Installment_StatusEnum.Paid_Late &&
                    i.Bnpl_Installment_Status != BNPL_Installment_StatusEnum.Refunded)
                .Where(i => i.RemainingBalance > 0)
                .Where(i => i.Installment_DueDate <= now)
                .OrderBy(i => i.InstallmentNo)
                .ToList();

            foreach (var inst in installmentsToProcess)
            {
                //Apply : Helper method for handle single installment update
                var breakdown = ApplyPaymentToSingleInstallment(inst, now, ref remainingArrears, ref remainingLateInterest, ref remainingBase, ref remainingOverpayment);
                resultDto.PerInstallmentBreakdown.Add(breakdown);
                updatedInstallments.Add(inst);

                resultDto.InstallmentId = inst.InstallmentID;
                resultDto.AppliedToArrears += breakdown.AppliedToArrears;
                resultDto.AppliedToLateInterest += breakdown.AppliedToLateInterest;
                resultDto.AppliedToBase += breakdown.AppliedToBase;
                resultDto.OverPayment += breakdown.OverPayment;
                resultDto.NewStatus = inst.Bnpl_Installment_Status.ToString();
            }

            return (resultDto, updatedInstallments);
        }

        //Helper method to handle single installment update
        private BnplPerInstallmentPaymentBreakdownResultDto ApplyPaymentToSingleInstallment(BNPL_Installment installment, DateTime today, ref decimal remainingArrears, ref decimal remainingLateInterest, ref decimal remainingBase, ref decimal remainingOverpayment)
        {
            var breakdown = new BnplPerInstallmentPaymentBreakdownResultDto
            {
                InstallmentId = installment.InstallmentID
            };

            // 1.APPLY ARREARS
            if (remainingArrears > 0)
            {
                decimal arrearsDue = installment.Installment_BaseAmount - installment.AmountPaid_AgainstBase;
                decimal apply = Math.Min(arrearsDue, remainingArrears);

                installment.AmountPaid_AgainstBase += apply;
                remainingArrears -= apply;

                breakdown.AppliedToArrears = apply;
            }

            // 2. APPLY LATE INTEREST
            if (remainingLateInterest > 0)
            {
                decimal lateDue = installment.LateInterest - installment.AmountPaid_AgainstLateInterest;
                decimal apply = Math.Min(lateDue, remainingLateInterest);

                installment.AmountPaid_AgainstLateInterest += apply;
                remainingLateInterest -= apply;

                breakdown.AppliedToLateInterest = apply;
            }

            // 3. APPLY BASE
            if (remainingBase > 0)
            {
                decimal baseDue = installment.Installment_BaseAmount - installment.AmountPaid_AgainstBase;
                decimal apply = Math.Min(baseDue, remainingBase);

                installment.AmountPaid_AgainstBase += apply;
                remainingBase -= apply;

                breakdown.AppliedToBase = apply;
            }

            // 4. APPLY OVERPAYMENT
            if (remainingOverpayment > 0)
            {
                decimal baseDue = installment.Installment_BaseAmount - installment.AmountPaid_AgainstBase;
                decimal apply = Math.Min(baseDue, remainingOverpayment);

                installment.AmountPaid_AgainstBase += apply;
                remainingOverpayment -= apply;

                breakdown.OverPayment = apply;
            }

            installment.TotalDueAmount = installment.Installment_BaseAmount + installment.LateInterest;
            installment.LastPaymentDate = today;

            //Apply helper method to determine installment status
            installment.Bnpl_Installment_Status = DetermineInstallmentStatus(installment, today);

            return breakdown;
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