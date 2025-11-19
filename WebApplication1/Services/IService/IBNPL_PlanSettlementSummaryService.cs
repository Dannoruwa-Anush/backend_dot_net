using WebApplication1.DTOs.RequestDto.BnplSnapshotPayingSimulation;
using WebApplication1.DTOs.ResponseDto.BnplSnapshotPayingSimulation;
using WebApplication1.Models;

namespace WebApplication1.Services.IService
{
    public interface IBNPL_PlanSettlementSummaryService
    {
        //CRUD operations

        //Custom Query Operations
        Task<BNPL_PlanSettlementSummary> GenerateSettlementAsync(int planId);

        //simulator
        Task<BnplSnapshotPayingSimulationResultDto> SimulateBnplPlanSettlementAsync(BnplSnapshotPayingSimulationRequestDto request);
    }
}