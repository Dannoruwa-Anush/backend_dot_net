using WebApplication1.DTOs.RequestDto.BnplCal;
using WebApplication1.DTOs.ResponseDto.BnplCal;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

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
        //Task<BNPL_PLAN?> BuildBNPL_PlanUpdateRequestAsync(int id, BNPL_PLAN bNPL_Plan);
    }
}