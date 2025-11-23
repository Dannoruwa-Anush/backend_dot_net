using WebApplication1.DTOs.RequestDto.Payment;
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
        private readonly IAppUnitOfWork _unitOfWork;
        private readonly ICustomerOrderRepository _customerOrderRepository;
        private readonly IBNPL_PlanTypeRepository _bNPL_PlanTypeRepository;
        private readonly IBNPL_PlanRepository _bNPL_PlanRepository;
        private readonly IBNPL_PlanSettlementSummaryRepository _bNPL_PlanSettlementSummaryRepository;
        private readonly IBNPL_PlanSettlementSummaryService _bnpl_planSettlementSummaryService;

        //logger: for auditing
        private readonly ILogger<BNPL_InstallmentServiceImpl> _logger;

        // Constructor
        public BNPL_InstallmentServiceImpl(IBNPL_InstallmentRepository repository, IAppUnitOfWork unitOfWork, ICustomerOrderRepository customerOrderRepository, IBNPL_PlanTypeRepository bNPL_PlanTypeRepository, IBNPL_PlanRepository bNPL_PlanRepository, IBNPL_PlanSettlementSummaryRepository bNPL_PlanSettlementSummaryRepository, IBNPL_PlanSettlementSummaryService bnpl_planSettlementSummaryService, ILogger<BNPL_InstallmentServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _unitOfWork = unitOfWork;
            _customerOrderRepository = customerOrderRepository;
            _bNPL_PlanTypeRepository = bNPL_PlanTypeRepository;
            _bNPL_PlanRepository = bNPL_PlanRepository;
            _bNPL_PlanSettlementSummaryRepository = bNPL_PlanSettlementSummaryRepository;
            _bnpl_planSettlementSummaryService = bnpl_planSettlementSummaryService;
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

        //Handle : Overdue Installments (LateIntrest + Arreas)
        public async Task ApplyLateInterestForAllPlansAsync()
        {
            var activePlans = await _bNPL_PlanRepository.GetAllActiveAsync();
            if (activePlans == null || !activePlans.Any())
                throw new Exception("ACtive plans not found");

            // Use single transaction for all plans
            await _unitOfWork.BeginTransactionAsync(); ;
            try
            {
                var today = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

                foreach (var plan in activePlans)
                {
                    await HandleOverdueInstallmentsAsync(plan.Bnpl_PlanID, today);
                }

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to apply late interest for plans");
                throw;
            }
        }

        // Helper: Handle Overdue Installments 
        private async Task HandleOverdueInstallmentsAsync(int planId, DateTime today)
        {
            var overdueInstallments = await _repository.GetAllUnsettledInstallmentUpToDateAsync(planId, today);

            if (overdueInstallments == null || !overdueInstallments.Any())
                throw new Exception("Overdue installments not found.");

            var bnplPlan = await _bNPL_PlanRepository.GetByIdAsync(planId);
            if (bnplPlan == null)
                throw new Exception("BNPL plan not found.");

            decimal lateInterestRatePerDay = bnplPlan.BNPL_PlanType!.LatePayInterestRatePerDay;

            foreach (var inst in overdueInstallments)
            {
                if (inst.Installment_DueDate < today && inst.TotalPaid < inst.TotalDueAmount)
                {
                    int overdueDays = Math.Max((today - inst.Installment_DueDate).Days, 1);

                    //Late interest only for remaining part
                    decimal lateInterest = inst.RemainingBalance * lateInterestRatePerDay * overdueDays;

                    inst.LateInterest += lateInterest;
                    inst.LastLateInterestAppliedDate = today;
                    inst.Bnpl_Installment_Status = BNPL_Installment_StatusEnum.Overdue;
                }
            }

            await BuildBnplInstallmetStatusUpdateRequestAsync(overdueInstallments, BNPL_Installment_StatusEnum.Overdue, today);

            // Snapshot after modifications
            var snapshot = await _bnpl_planSettlementSummaryService.BuildSettlementGenerateRequestAsync(planId);
            await _bNPL_PlanSettlementSummaryRepository.AddAsync(snapshot);

        }

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
                    CreatedAt = now,
                    Bnpl_Installment_Status = BNPL_Installment_StatusEnum.Pending
                });
            }

            await _repository.AddRangeAsync(installments);

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

        //Payment : Main Driver
        public async Task<(BnplInstallmentPaymentResultDto Result, List<BNPL_Installment> UpdatedInstallments)> BuildBnplInstallmentSettlementAsync(PaymentRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var order = await _customerOrderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
                throw new InvalidOperationException("Customer order not found.");

            var planId = order.BNPL_PLAN!.Bnpl_PlanID;
            var today = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            var installments = await _repository.GetAllUnsettledInstallmentUpToDateAsync(planId, today);

            if (!installments.Any())
                throw new InvalidOperationException("No unsettled installments found.");

            var response = new BnplInstallmentPaymentResultDto();
            decimal remaining = request.PaymentAmount;

            foreach (var inst in installments)
            {
                if (remaining <= 0)
                    break;

                var breakdown = ApplyPaymentToSingleInstallmentLogic(inst, ref remaining);
                response.PerInstallmentBreakdown.Add(breakdown);
            }

            // Return both breakdown + modified entities
            return (response, installments);
        }

        // Helper Method : apply payment for an installment
        private BnplPerInstallmentPaymentBreakdownResultDto ApplyPaymentToSingleInstallmentLogic(
            BNPL_Installment inst,
            ref decimal remainingPayment)
        {
            var breakdown = new BnplPerInstallmentPaymentBreakdownResultDto
            {
                InstallmentId = inst.InstallmentID
            };

            // ===== STEP 1 – Arrears =====
            decimal arrears = Math.Max(inst.Installment_BaseAmount - inst.AmountPaid_AgainstBase, 0m);

            if (arrears > 0 && remainingPayment > 0)
            {
                var applied = Math.Min(arrears, remainingPayment);
                inst.AmountPaid_AgainstArrears += applied;
                remainingPayment -= applied;
                breakdown.AppliedToArrears = applied;
            }

            // ===== STEP 2 – Late Interest =====
            if (inst.LateInterest > 0 && remainingPayment > 0)
            {
                var applied = Math.Min(inst.LateInterest, remainingPayment);
                inst.LateInterest -= applied;
                inst.AmountPaid_AgainstLateInterest += applied;
                remainingPayment -= applied;
                breakdown.AppliedToLateInterest = applied;
            }

            // ===== STEP 3 – Base Installment =====
            decimal baseRemaining = Math.Max(inst.Installment_BaseAmount - inst.AmountPaid_AgainstBase, 0m);

            if (baseRemaining > 0 && remainingPayment > 0)
            {
                var applied = Math.Min(baseRemaining, remainingPayment);
                inst.AmountPaid_AgainstBase += applied;
                remainingPayment -= applied;
                breakdown.AppliedToBase = applied;
            }

            // ===== STEP 4 – Overpayment =====
            if (remainingPayment > 0)
            {
                inst.OverPaymentCarried += remainingPayment;
                breakdown.OverPayment = remainingPayment;
                remainingPayment = 0;
            }

            inst.LastPaymentDate = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            // ===== STATUS UPDATE =====
            bool fullyPaid = inst.TotalPaid >= inst.TotalDueAmount;
            bool overdue = inst.Installment_DueDate < TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            if (fullyPaid)
            {
                inst.Bnpl_Installment_Status =
                    overdue ? BNPL_Installment_StatusEnum.Paid_Late
                            : BNPL_Installment_StatusEnum.Paid_OnTime;
            }
            else
            {
                if (inst.TotalPaid > 0)
                {
                    inst.Bnpl_Installment_Status =
                        overdue ? BNPL_Installment_StatusEnum.PartiallyPaid_Late
                                : BNPL_Installment_StatusEnum.PartiallyPaid_OnTime;
                }
                else
                {
                    inst.Bnpl_Installment_Status =
                        overdue ? BNPL_Installment_StatusEnum.Overdue
                                : BNPL_Installment_StatusEnum.Pending;
                }
            }

            breakdown.NewStatus = inst.Bnpl_Installment_Status.ToString();
            return breakdown;
        }

        public async Task BuildBnplInstallmetStatusUpdateRequestAsync(ICollection<BNPL_Installment> installments, BNPL_Installment_StatusEnum newStatus, DateTime now)
        {
            foreach (var installment in installments)
            {
                bool baseChanged = installment.AmountPaid_AgainstBase > 0;
                bool arrearsChanged = installment.AmountPaid_AgainstArrears > 0;
                bool lateChanged = installment.AmountPaid_AgainstLateInterest > 0;

                if (baseChanged || arrearsChanged || lateChanged)
                    installment.LastPaymentDate = now;

                if (installment.LateInterest > 0)
                    installment.LastLateInterestAppliedDate = now;

                installment.Bnpl_Installment_Status = newStatus;

                if (newStatus == BNPL_Installment_StatusEnum.Refunded)
                    installment.RefundDate = now;
            }

            await _repository.UpdateRangeAsync(installments);
        }
    }
}