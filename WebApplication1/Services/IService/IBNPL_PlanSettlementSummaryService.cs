using WebApplication1.Models;

namespace WebApplication1.Services.IService
{
    public interface IBNPL_PlanSettlementSummaryService
    {
        //CRUD operations
        Task<BNPL_PlanSettlementSummary> AddBNPL_PlanAsync(BNPL_PlanSettlementSummary bNPL_PlanSettlementSummary);

        //Custom Query Operations
        Task<BNPL_PlanSettlementSummary> GenerateSettlementAsync(int planId);
    }
}