using WebApplication1.Models;

namespace WebApplication1.Repositories.IRepository
{
    public interface IBNPL_PlanSettlementSummaryRepository
    {
        //CRUD Operations
        Task AddRangeAsync(List<BNPL_PlanSettlementSummary> snapshots);

        //Custom Query Operations
        Task<BNPL_PlanSettlementSummary?> GetLatestSnapshotAsync(int planId);
        Task<BNPL_PlanSettlementSummary?> GetLatestSnapshotWithOrderDetailsAsync(int orderId);
        Task MarkPreviousSnapshotsAsNotLatestAsync(int planId);
        Task MarkPreviousSnapshotsAsNotLatestBatchAsync(List<int> planIds);
    }
}