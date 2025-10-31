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
        Task<BNPL_PLAN> AddBNPL_PlanAsync(BNPL_PLAN bNPL_Plan);
        Task<BNPL_PLAN?> UpdateBNPL_PlanAsync(int id, BnplStatusEnum newStatus);

        //Custom Query Operations
        Task<BNPLInstallmentCalculatorResponseDto> CalculateBNPL_PlanAmountPerInstallmentAsync(BNPLInstallmentCalculatorRequestDto request);
        Task<PaginationResultDto<BNPL_PLAN>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? planStatusId = null, string? searchKey = null);
    }
}