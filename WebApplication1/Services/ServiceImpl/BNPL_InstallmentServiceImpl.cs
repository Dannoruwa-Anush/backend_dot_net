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
        private readonly IAppUnitOfWork _unitOfWork;

        private readonly ICustomerOrderRepository _customerOrderRepository;
        private readonly IBNPL_PlanTypeRepository _bNPL_PlanTypeRepository;
        private readonly IBNPL_PlanRepository _bNPL_PlanRepository;
        private readonly IBNPL_InstallmentRepository _bNPL_InstallmentRepository;
        private readonly IBNPL_PlanSettlementSummaryService _bnpl_planSettlementSummaryService;

        //logger: for auditing
        private readonly ILogger<BNPL_InstallmentServiceImpl> _logger;

        // Constructor
        public BNPL_InstallmentServiceImpl(IBNPL_InstallmentRepository repository, IAppUnitOfWork unitOfWork, ICustomerOrderRepository customerOrderRepository, IBNPL_PlanTypeRepository bNPL_PlanTypeRepository, IBNPL_PlanRepository bNPL_PlanRepository, IBNPL_PlanSettlementSummaryService bnpl_planSettlementSummaryService, IBNPL_InstallmentRepository bNPL_InstallmentRepository, ILogger<BNPL_InstallmentServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _unitOfWork = unitOfWork;
            _customerOrderRepository = customerOrderRepository;
            _bNPL_PlanTypeRepository = bNPL_PlanTypeRepository;
            _bNPL_PlanRepository = bNPL_PlanRepository;
            _bNPL_InstallmentRepository = bNPL_InstallmentRepository;
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

            if (!activePlans.Any())
            {
                _logger.LogInformation("No active BNPL plans found.");
                return;
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                DateTime today = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

                foreach (var plan in activePlans)
                {
                    var updatedInstallments = await ApplyLateInterestToPlanAsync(plan.Bnpl_PlanID, today);

                    if (updatedInstallments != null)
                    {
                        _bnpl_planSettlementSummaryService.BuildSettlementGenerateRequestForPlanAsync(plan);

                        _logger.LogInformation($"Snapshot created for Plan={plan.Bnpl_PlanID}");
                    }
                    else
                    {
                        _logger.LogInformation($"No late interest applied for Plan={plan.Bnpl_PlanID}");
                    }
                }

                // Commit all changes in a single transaction
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to apply late interest for BNPL plans");
                throw;
            }
        }

        //Helper : interest for a single plan
        private async Task<List<BNPL_Installment>?> ApplyLateInterestToPlanAsync(int planId, DateTime today)
        {
            // Fetch all overdue/unsettled installments (tracked)
            var installments = await _bNPL_InstallmentRepository
                .GetAllUnsettledInstallmentUpToDateAsync(planId, today);

            if (!installments.Any())
                return null;

            var plan = await _bNPL_PlanRepository.GetByIdAsync(planId)
                       ?? throw new Exception($"BNPL plan {planId} not found.");

            decimal lateInterestRatePerDay = plan.BNPL_PlanType!.LatePayInterestRatePerDay;

            foreach (var inst in installments)
            {
                //if (inst.RemainingBalance <= 0)
                //continue;

                // Calculate days since last interest applied
                DateTime lastApplied = inst.LastLateInterestAppliedDate ?? inst.Installment_DueDate;
                int overdueDays = Math.Max((today.Date - lastApplied.Date).Days, 0);

                if (overdueDays == 0)
                    continue;

                // Apply late interest
                //inst.LateInterest += inst.RemainingBalance * lateInterestRatePerDay * overdueDays;
                inst.LastLateInterestAppliedDate = today;
                inst.Bnpl_Installment_Status = BNPL_Installment_StatusEnum.Overdue;
            }

            return installments;
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

        //Payment : Main Driver
        public (BnplInstallmentPaymentResultDto Result, List<BNPL_Installment> UpdatedInstallments)
    BuildBnplInstallmentSettlementAsync(
        List<BNPL_Installment> installments,
        BnplLastSnapshotSettledResultDto lastSnapshotSettledResult)
        {
            var sorted = installments.OrderBy(i => i.InstallmentNo).ToList();

            decimal payArrears = lastSnapshotSettledResult.TotalPaidArrears;
            decimal payInterest = lastSnapshotSettledResult.TotalPaidLateInterest;
            decimal payBase = lastSnapshotSettledResult.TotalPaidCurrentInstallmentBase;

            decimal carryForward = lastSnapshotSettledResult.OverPaymentCarriedToNextInstallment;

            DateTime now = DateTime.UtcNow;

            var breakdownList = new List<BnplPerInstallmentPaymentBreakdownResultDto>();

            foreach (var inst in sorted)
            {
                var breakdown = ApplyPaymentToSingleInstallmentLogic(
                    inst,
                    ref payArrears,
                    ref payInterest,
                    ref payBase,
                    ref carryForward,
                    now
                );

                breakdownList.Add(breakdown);
            }

            // Build high-level response
            var result = new BnplInstallmentPaymentResultDto
            {
                InstallmentId = sorted.First().InstallmentID,
                AppliedToArrears = lastSnapshotSettledResult.TotalPaidArrears,
                AppliedToLateInterest = lastSnapshotSettledResult.TotalPaidLateInterest,
                AppliedToBase = lastSnapshotSettledResult.TotalPaidCurrentInstallmentBase,
                OverPayment = carryForward,
                PerInstallmentBreakdown = breakdownList
            };

            // Set overall status as status of the first installment
            result.NewStatus = sorted.First().Bnpl_Installment_Status.ToString();

            return (result, sorted);
        }

        //Helper method : for single installment
        private BnplPerInstallmentPaymentBreakdownResultDto ApplyPaymentToSingleInstallmentLogic(
            BNPL_Installment inst,
            ref decimal payArrears,
            ref decimal payInterest,
            ref decimal payCurrentBase,
            ref decimal carryForward,
            DateTime now)
        {
            var result = new BnplPerInstallmentPaymentBreakdownResultDto
            {
                InstallmentId = inst.InstallmentID
            };

            // -----------------------------------------------------------
            // APPLY CARRY-FORWARD FIRST
            // -----------------------------------------------------------
            inst.OverPaymentCarriedFromPreviousInstallment += carryForward;
            carryForward = 0;

            decimal remainingBase =
                Math.Max(0, inst.Installment_BaseAmount - inst.AmountPaid_AgainstBase);

            decimal remainingInterest =
                Math.Max(0, inst.LateInterest - inst.AmountPaid_AgainstLateInterest);

            // -----------------------------------------------------------
            // 1. APPLY ARREARS (only overdue installments)
            // -----------------------------------------------------------
            if (inst.Installment_DueDate < now && payArrears > 0 && remainingBase > 0)
            {
                decimal pay = Math.Min(payArrears, remainingBase);
                inst.AmountPaid_AgainstBase += pay;

                result.AppliedToArrears = pay;
                payArrears -= pay;
                remainingBase -= pay;
            }

            // -----------------------------------------------------------
            // 2. APPLY LATE INTEREST
            // -----------------------------------------------------------
            if (payInterest > 0 && remainingInterest > 0)
            {
                decimal pay = Math.Min(payInterest, remainingInterest);
                inst.AmountPaid_AgainstLateInterest += pay;

                result.AppliedToLateInterest = pay;
                payInterest -= pay;
                remainingInterest -= pay;
            }

            // -----------------------------------------------------------
            // 3. APPLY CURRENT INSTALLMENT BASE (only for upcoming installment)
            // -----------------------------------------------------------
            if (inst.Installment_DueDate >= now && payCurrentBase > 0 && remainingBase > 0)
            {
                decimal pay = Math.Min(payCurrentBase, remainingBase);
                inst.AmountPaid_AgainstBase += pay;

                result.AppliedToBase = pay;
                payCurrentBase -= pay;
                remainingBase -= pay;
            }

            // -----------------------------------------------------------
            // 4. CHECK FOR OVERPAYMENT
            // -----------------------------------------------------------
            decimal totalRemainingDue = remainingBase + remainingInterest;

            if (totalRemainingDue <= 0)
            {
                decimal overpay = Math.Abs(totalRemainingDue);
                result.OverPayment = overpay;
                carryForward += overpay;
            }

            // -----------------------------------------------------------
            // 5. UPDATE STATUS
            // -----------------------------------------------------------
            if (remainingBase == 0 && remainingInterest == 0)
            {
                inst.Bnpl_Installment_Status =
                    inst.Installment_DueDate >= now
                    ? BNPL_Installment_StatusEnum.Paid_OnTime
                    : BNPL_Installment_StatusEnum.Paid_Late;
            }
            else if (inst.AmountPaid_AgainstBase > 0 || inst.AmountPaid_AgainstLateInterest > 0)
            {
                inst.Bnpl_Installment_Status =
                    inst.Installment_DueDate >= now
                    ? BNPL_Installment_StatusEnum.PartiallyPaid_OnTime
                    : BNPL_Installment_StatusEnum.PartiallyPaid_Late;
            }
            else if (inst.Installment_DueDate < now)
            {
                inst.Bnpl_Installment_Status = BNPL_Installment_StatusEnum.Overdue;
            }

            result.NewStatus = inst.Bnpl_Installment_Status.ToString();

            // -----------------------------------------------------------
            // 6. UPDATE TOTAL DUE
            // -----------------------------------------------------------
            inst.TotalDueAmount =
                (inst.Installment_BaseAmount - inst.OverPaymentCarriedFromPreviousInstallment)
                + inst.LateInterest
                - inst.AmountPaid_AgainstBase
                - inst.AmountPaid_AgainstLateInterest;

            if (inst.TotalDueAmount < 0)
                inst.TotalDueAmount = 0;

            inst.LastPaymentDate = now;

            return result;
        }
    }
}