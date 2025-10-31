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
        Task<IEnumerable<BNPL_PLAN>> GetAllAsync();
        Task<BNPL_PLAN?> GetByIdAsync(int id);
        Task<BNPL_PLAN> AddAsync(BNPL_PLAN bNPL_Plan);
        Task<BNPL_PLAN?> UpdateAsync(int id, BnplStatusEnum newStatus);

        //Custom Query Operations
        Task<BNPLInstallmentCalculatorResponseDto> CalculateAmountPerInstallmentAsync(BNPLInstallmentCalculatorRequestDto request);
        Task<PaginationResultDto<BNPL_PLAN>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? planStatusId = null, string? searchKey = null);
    }
}