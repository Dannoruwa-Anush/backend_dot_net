using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.Utils.Helpers;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.ServiceImpl
{
    public class BNPL_PlanSettlementSummaryServiceImpl : IBNPL_PlanSettlementSummaryService
    {
        private readonly IBNPL_PlanSettlementSummaryRepository _repository;
        private readonly IBNPL_InstallmentRepository _bNPL_InstallmentRepository;

        // Constructor
        public BNPL_PlanSettlementSummaryServiceImpl(IBNPL_PlanSettlementSummaryRepository repository, IBNPL_InstallmentRepository bNPL_InstallmentRepository)
        {
            // Dependency injection
            _repository = repository;
            _bNPL_InstallmentRepository = bNPL_InstallmentRepository;
        }

        //CRUD operations
        public async Task<BNPL_PlanSettlementSummary> AddBNPL_PlanAsync(BNPL_PlanSettlementSummary snapshot)
        {
            // Mark old snapshots
            await _repository.MarkPreviousSnapshotsAsNotLatestAsync(snapshot.Bnpl_PlanID);

            // Insert new snapshot
            await _repository.AddAsync(snapshot);

            return snapshot;
        }

        //Custom Query Operations
        public async Task<BNPL_PlanSettlementSummary> GenerateSettlementAsync(int planId)
        {
            var today = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            // Step 1: Build the settlement snapshot
            var snapshot = await BuildSettlementSummaryAsync(planId, today);

            if (snapshot == null)
                throw new Exception("Settlement snapshot is null");

            // Step 2: Save to DB
            return await AddBNPL_PlanAsync(snapshot);
        }

        // Helper method : BuildSettlementSummaryAsync
        private async Task<BNPL_PlanSettlementSummary> BuildSettlementSummaryAsync(int planId, DateTime asOfDate)
        {
            // Get all unsettled installments up to date
            var unsettled = await _bNPL_InstallmentRepository
                .GetAllUnsettledInstallmentUpToDateAsync(planId, asOfDate);

            if (unsettled == null || !unsettled.Any())
                throw new Exception("Unsettled installments not found");

            // ---------- ACCUMULATIONS ----------
            var totalBaseAmount = unsettled.Sum(i => i.Installment_BaseAmount);
            var totalLateInterest = unsettled.Sum(i => i.LateInterest);
            var totalOverPayment = unsettled.Sum(i => i.OverPaymentCarried);

            var totalArrears = unsettled.Sum(i => (i.TotalDueAmount - i.AmountPaid));

            var totalPayable = totalArrears + totalLateInterest - totalOverPayment;

            var currentInstallmentNo = unsettled.Max(i => i.InstallmentNo);

            // ---------- RETURN POPULATED SNAPSHOT MODEL ----------
            return new BNPL_PlanSettlementSummary
            {
                Bnpl_PlanID = planId,
                CurrentInstallmentNo = currentInstallmentNo,
                TotalCurrentArrears = totalArrears,
                TotalCurrentLateInterest = totalLateInterest,
                InstallmentBaseAmount = totalBaseAmount,
                TotalCurrentOverPayment = totalOverPayment,
                TotalPayableSettlement = totalPayable,
                Bnpl_PlanSettlementSummary_Status = BNPL_PlanSettlementSummary_StatusEnum.Active,
                IsLatest = true,
            };
        }
    }
}