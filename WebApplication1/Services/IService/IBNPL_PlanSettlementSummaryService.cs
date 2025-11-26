using WebApplication1.DTOs.RequestDto.BnplSnapshotPayingSimulation;
using WebApplication1.DTOs.ResponseDto.BnplSnapshotPayingSimulation;
using WebApplication1.Models;

namespace WebApplication1.Services.IService
{
    public interface IBNPL_PlanSettlementSummaryService
    {
        //Custom Query Operations
        Task<BNPL_PlanSettlementSummary?> GetLatestSnapshotWithOrderDetailsAsync(int orderId);
        
        //simulator
        Task<BnplSnapshotPayingSimulationResultDto> SimulateBnplPlanSettlementAsync(BnplSnapshotPayingSimulationRequestDto request);
        
        //Shared Internal Operations Used by Multiple Repositories
        (BnplLatestSnapshotSettledResultDto, BNPL_PlanSettlementSummary)BuildBNPL_PlanLatestSettlementSummaryUpdateRequestAsync(BNPL_PlanSettlementSummary latestSnapshot, decimal paymentAmount);

        BNPL_PlanSettlementSummary? BuildSettlementGenerateRequestForPlanAsync(BNPL_PLAN existingPlan);
    }
}