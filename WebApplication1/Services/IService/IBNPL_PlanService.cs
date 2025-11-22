using WebApplication1.DTOs.RequestDto.BnplCal;
using WebApplication1.DTOs.ResponseDto.BnplCal;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.IService
{
    public interface IBNPL_PlanService
    {
        //CRUD operations
        Task<IEnumerable<BNPL_PLAN>> GetAllBNPL_PlansAsync();
        Task<BNPL_PLAN?> GetBNPL_PlanByIdAsync(int id);

        //Custom Query Operations
        Task<PaginationResultDto<BNPL_PLAN>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? planStatusId = null, string? searchKey = null);
        Task<BNPL_PLAN?> GetByOrderIdAsync(int OrderId);

        //calculator
        Task<BNPLInstallmentCalculatorResponseDto> CalculateBNPL_PlanAmountPerInstallmentAsync(BNPLInstallmentCalculatorRequestDto request);

        //Shared Internal Operations Used by Multiple Repositories
        Task<BNPL_PLAN> BuildBnpl_PlanAddRequestAsync(BNPL_PLAN bNPL_Plan);

        Task BuildBnplPlanStatusUpdateRequestAsync(BNPL_PLAN plan, BnplStatusEnum planStatus, DateTime now);
    }
}