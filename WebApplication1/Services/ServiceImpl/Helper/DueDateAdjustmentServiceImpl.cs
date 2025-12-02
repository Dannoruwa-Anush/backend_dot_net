using WebApplication1.DTOs.ResponseDto.LateInterest;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.Services.IService.Helper;
using WebApplication1.UOW.IUOW;
using WebApplication1.Utils.Helpers;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.ServiceImpl.Helper
{
    public class DueDateAdjustmentServiceImpl : IDueDateAdjustmentService
    {
        private readonly IAppUnitOfWork _unitOfWork;
        private readonly IBNPL_PlanRepository _bNPL_PlanRepository;
        private readonly IBNPL_InstallmentRepository _bNPL_InstallmentRepository;
        private readonly IBNPL_PlanSettlementSummaryService _bnpl_planSettlementSummaryService;

        //logger: for auditing
        private readonly ILogger<DueDateAdjustmentServiceImpl> _logger;

        // Constructor
        public DueDateAdjustmentServiceImpl(IAppUnitOfWork unitOfWork, IBNPL_PlanRepository planRepo, IBNPL_PlanSettlementSummaryService settlementService, IBNPL_InstallmentRepository installmentRepo, ILogger<DueDateAdjustmentServiceImpl> logger)
        {
            // Dependency injection
            _unitOfWork = unitOfWork;
            _bNPL_PlanRepository = planRepo;
            _bNPL_InstallmentRepository = installmentRepo;
            _bnpl_planSettlementSummaryService = settlementService;
            _logger = logger;
        }

        //Main Driver Method : Process Due Date Adjustments
        public async Task ProcessDueDateAdjustmentsAsync()
        {
            var activePlans = await _bNPL_PlanRepository.GetAllActiveAsync();

            if (!activePlans.Any())
            {
                _logger.LogInformation("No active BNPL plans found.");
                return;
            }

            DateTime today = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var plan in activePlans)
                {
                    bool lateInterestApplied = await ApplyLateInterestToPlanAsync(plan.Bnpl_PlanID, today);
                    bool overpaymentApplied = await ApplyOverpaymentToNextInstallmentAsync(plan.Bnpl_PlanID, today);

                    if (lateInterestApplied || overpaymentApplied || !IsPlanCompleted(plan))
                    {
                        _bnpl_planSettlementSummaryService.BuildSettlementGenerateRequestForPlanAsync(plan);

                        _logger.LogInformation($"Snapshot created for Plan={plan.Bnpl_PlanID}; " + $"InterestApplied={lateInterestApplied}, OverpayApplied={overpaymentApplied}");
                    }
                }

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Due date adjustment failed");
                throw;
            }
        }

        //Helper method : for late payment interest
        private async Task<bool> ApplyLateInterestToPlanAsync(int planId, DateTime today)
        {
            var installments = await _bNPL_InstallmentRepository.GetAllUnsettledInstallmentUpToDateAsync(planId, today);

            if (!installments.Any())
                return false;

            var plan = await _bNPL_PlanRepository.GetByIdAsync(planId)
                       ?? throw new Exception($"BNPL plan {planId} not found.");

            decimal ratePerDay = plan.BNPL_PlanType!.LatePayInterestRatePerDay;
            bool anyApplied = false;

            foreach (var inst in installments)
            {
                //call helper method : CalculateLateInterest
                var calc = CalculateLateInterest(inst, ratePerDay, today);

                if (calc.OverdueDays <= 0)
                    continue;

                // Update the installment
                inst.LateInterest = calc.NewLateInterestTotal;
                inst.TotalDueAmount = calc.NewTotalDueAmount;
                inst.LastLateInterestAppliedDate = today;
                inst.Bnpl_Installment_Status = BNPL_Installment_StatusEnum.Overdue;

                anyApplied = true;
            }

            return anyApplied;
        }

        //Helper method : single installment late interest calculator
        private LateInterestCalculationResultDto CalculateLateInterest(BNPL_Installment inst, decimal ratePerDay, DateTime today)
        {
            DateTime lastAppliedDate = inst.LastLateInterestAppliedDate ?? inst.Installment_DueDate;

            int overdueDays = Math.Max((today.Date - lastAppliedDate.Date).Days, 0);

            if (overdueDays <= 0)
            {
                return new LateInterestCalculationResultDto
                {
                    OverdueDays = 0,
                    UnpaidBase = 0,
                    InterestToAdd = 0,
                    NewLateInterestTotal = inst.LateInterest,
                    NewTotalDueAmount = inst.TotalDueAmount
                };
            }

            decimal unpaidBase = inst.Installment_BaseAmount - inst.AmountPaid_AgainstBase;

            if (unpaidBase < 0)
                unpaidBase = 0;

            decimal interestToAdd = Math.Round(unpaidBase * ratePerDay * overdueDays, 2, MidpointRounding.AwayFromZero);

            decimal newLateInterestTotal = inst.LateInterest + interestToAdd;
            decimal newTotalDueAmount = Math.Round(unpaidBase + newLateInterestTotal, 2, MidpointRounding.AwayFromZero);

            return new LateInterestCalculationResultDto
            {
                OverdueDays = overdueDays,
                UnpaidBase = unpaidBase,
                InterestToAdd = interestToAdd,
                NewLateInterestTotal = newLateInterestTotal,
                NewTotalDueAmount = newTotalDueAmount
            };
        }

        //Helper method : apply overpayment to next installment
        private async Task<bool> ApplyOverpaymentToNextInstallmentAsync(int planId, DateTime today)
        {
            var installments = await _bNPL_InstallmentRepository.GetAllByPlanIdAsync(planId);

            var ordered = installments.OrderBy(i => i.InstallmentNo).ToList();

            // Get the installment where due date is today or yesterday
            var source = ordered.FirstOrDefault(i =>
                i.Installment_DueDate.Date == today.Date ||
                i.Installment_DueDate.Date == today.AddDays(-1).Date);

            if (source == null)
                return false;

            decimal overpay = source.OverpaymentCarriedToNextMonth;

            if (overpay <= 0)
                return false;

            // Find the next unpaid installment after this one
            var next = ordered.FirstOrDefault(i =>
                i.InstallmentNo > source.InstallmentNo &&
                i.Bnpl_Installment_Status != BNPL_Installment_StatusEnum.Paid_OnTime &&
                i.Bnpl_Installment_Status != BNPL_Installment_StatusEnum.Paid_Late &&
                i.Bnpl_Installment_Status != BNPL_Installment_StatusEnum.Refunded);

            if (next == null)
                return false;

            decimal baseDue = next.Installment_BaseAmount - next.AmountPaid_AgainstBase;
            decimal applyAmount = Math.Min(baseDue, overpay);

            next.AmountPaid_AgainstBase += applyAmount;
            next.LastPaymentDate = today;

            // update remaining overpayment
            source.OverpaymentCarriedToNextMonth = overpay - applyAmount;

            return true;
        }

        //Helper method : to check whether the plan is completed or not
        private bool IsPlanCompleted(BNPL_PLAN plan)
        {
            return plan.Bnpl_Status == BnplStatusEnum.Completed;
        }
    }
}
