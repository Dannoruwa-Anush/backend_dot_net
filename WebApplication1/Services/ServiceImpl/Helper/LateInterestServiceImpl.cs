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
    public class LateInterestServiceImpl : ILateInterestService
    {
        private readonly IAppUnitOfWork _unitOfWork;

        private readonly IBNPL_PlanRepository _bNPL_PlanRepository;
        private readonly IBNPL_InstallmentRepository _bNPL_InstallmentRepository;
        private readonly IBNPL_PlanSettlementSummaryService _bnpl_planSettlementSummaryService;

        //logger: for auditing
        private readonly ILogger<LateInterestServiceImpl> _logger;

        // Constructor
        public LateInterestServiceImpl(IAppUnitOfWork unitOfWork, IBNPL_PlanRepository bNPL_PlanRepository, IBNPL_PlanSettlementSummaryService bnpl_planSettlementSummaryService, IBNPL_InstallmentRepository bNPL_InstallmentRepository, ILogger<LateInterestServiceImpl> logger)
        {
            // Dependency injection
            _unitOfWork = unitOfWork;
            _bNPL_PlanRepository = bNPL_PlanRepository;
            _bNPL_InstallmentRepository = bNPL_InstallmentRepository;
            _bnpl_planSettlementSummaryService = bnpl_planSettlementSummaryService;
            _logger = logger;
        }

        //Custom Query Operations
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
                        _bnpl_planSettlementSummaryService.BuildSettlementGenerateRequestForPlanAsync(plan, Bnpl_PlanSettlementSummary_TypeEnum.AfterLateInterest);

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

        //Helper :
        private async Task<List<BNPL_Installment>?> ApplyLateInterestToPlanAsync(int planId, DateTime today)
        {
            var installments = await _bNPL_InstallmentRepository
                .GetAllUnsettledInstallmentUpToDateAsync(planId, today);

            if (!installments.Any())
                return null;

            var plan = await _bNPL_PlanRepository.GetByIdAsync(planId)
                       ?? throw new Exception($"BNPL plan {planId} not found.");

            decimal ratePerDay = plan.BNPL_PlanType!.LatePayInterestRatePerDay;

            foreach (var inst in installments)
            {
                var calc = CalculateLateInterest(inst, ratePerDay, today);

                if (calc.OverdueDays == 0)
                    continue;

                inst.LateInterest = calc.NewLateInterestTotal;
                inst.TotalDueAmount = calc.NewTotalDueAmount;
                inst.LastLateInterestAppliedDate = today;
                inst.Bnpl_Installment_Status = BNPL_Installment_StatusEnum.Overdue;
            }
            return installments;
        }

        //Helper : interest for a single plan
        private LateInterestCalculationResultDto CalculateLateInterest(BNPL_Installment inst, decimal ratePerDay, DateTime today)
        {
            // --------------------------------------------
            // Determine overdue days
            // --------------------------------------------
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

            // --------------------------------------------
            // Calculate unpaid base (never negative)
            // --------------------------------------------
            decimal unpaidBase =
                inst.Installment_BaseAmount
                - inst.OverPaymentCarriedFromPreviousInstallment
                - inst.AmountPaid_AgainstBase;

            if (unpaidBase < 0)
                unpaidBase = 0;

            // --------------------------------------------
            // Calculate interest (rounded)
            // --------------------------------------------
            decimal interestToAdd = Math.Round(unpaidBase * ratePerDay * overdueDays, 2, MidpointRounding.AwayFromZero);

            // --------------------------------------------
            // New totals
            // --------------------------------------------
            decimal newLateInterestTotal = inst.LateInterest + interestToAdd;

            decimal newTotalDueAmount =
                Math.Round(unpaidBase + newLateInterestTotal, 2, MidpointRounding.AwayFromZero);

            return new LateInterestCalculationResultDto
            {
                OverdueDays = overdueDays,
                UnpaidBase = unpaidBase,
                InterestToAdd = interestToAdd,
                NewLateInterestTotal = newLateInterestTotal,
                NewTotalDueAmount = newTotalDueAmount
            };
        }
    }
}