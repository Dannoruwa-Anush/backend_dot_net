using WebApplication1.Models;

namespace WebApplication1.Repositories.IRepository
{
    public interface IBNPL_PlanSettlementSummaryRepository
    {
        //Custom Query Operations
        Task MarkPreviousSnapshotsAsNotLatestAsync(int planId);
        Task<BNPL_PlanSettlementSummary?> GetLatestSnapshotAsync(int planId);
        Task<BNPL_PlanSettlementSummary?> GetLatestSnapshotWithOrderDetailsAsync(int orderId);
    }
}