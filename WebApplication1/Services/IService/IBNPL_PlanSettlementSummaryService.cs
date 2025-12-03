using WebApplication1.DTOs.RequestDto.BnplSnapshotPayingSimulation;
using WebApplication1.DTOs.ResponseDto.BnplSnapshotPayingSimulation;
using WebApplication1.Models;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.IService
{
    public interface IBNPL_PlanSettlementSummaryService
    {
        //Custom Query Operations
        Task<BNPL_PlanSettlementSummary?> GetLatestSnapshotWithOrderDetailsAsync(int orderId);
        
        //simulator
        Task<BnplSnapshotPayingSimulationResultDto> SimulateBnplPlanSettlementAsync(BnplSnapshotPayingSimulationRequestDto request);
        
        //Shared Internal Operations Used by Multiple Repositories
        (BnplLatestSnapshotSettledResultDto snapshotResult, CustomerOrder updatedOrder) BuildBNPL_PlanLatestSettlementSummaryUpdateRequestAsync(CustomerOrder existingOrder, decimal paymentAmount);

        BNPL_PlanSettlementSummary? BuildSettlementGenerateRequestForPlanAsync(BNPL_PLAN existingPlan);
    }
}