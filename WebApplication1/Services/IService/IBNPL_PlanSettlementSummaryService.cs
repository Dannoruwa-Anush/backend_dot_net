using WebApplication1.DTOs.RequestDto.BnplSnapshotPayingSimulation;
using WebApplication1.DTOs.ResponseDto.BnplSnapshotPayingSimulation;
using WebApplication1.Models;

namespace WebApplication1.Services.IService
{
    public interface IBNPL_PlanSettlementSummaryService
    {
        //CRUD operations

        //Custom Query Operations
        
        //simulator
        Task<BnplSnapshotPayingSimulationResultDto> SimulateBnplPlanSettlementAsync(BnplSnapshotPayingSimulationRequestDto request);
        //Builds the object without DB Access
        Task<BNPL_PlanSettlementSummary> BuildSettlementGenerateRequestAsync(int planId);
    }
}