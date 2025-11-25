using WebApplication1.Models;

namespace WebApplication1.Repositories.IRepository
{
    public interface IBNPL_PlanSettlementSummaryRepository
    {
        //Custom Query Operations
        Task<IEnumerable<BNPL_PlanSettlementSummary>> GetAllByPlanIdAsync(int planId);
        Task<BNPL_PlanSettlementSummary?> GetLatestSnapshotAsync(int planId);
        Task<BNPL_PlanSettlementSummary?> GetLatestSnapshotWithOrderDetailsAsync(int orderId);
        Task MarkPreviousSnapshotsAsNotLatestAsync(int planId);
        Task MarkPreviousSnapshotsAsNotLatestBatchAsync(List<int> planIds);
    }
}