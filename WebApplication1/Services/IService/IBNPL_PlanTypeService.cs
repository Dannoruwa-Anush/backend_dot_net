using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Services.IService
{
    public interface IBNPL_PlanTypeService
    {
        //CRUD operations
        Task<IEnumerable<BNPL_PlanType>> GetAllBNPL_PlanTypesAsync();
        Task<BNPL_PlanType?> GetBNPL_PlanTypeByIdAsync(int id);
        Task<BNPL_PlanType> AddBNPL_PlanTypeAsync(BNPL_PlanType bNPL_PlanType);
        Task<BNPL_PlanType> UpdateBNPL_PlanTypeAsync(int id, BNPL_PlanType bNPL_PlanType);
        Task DeleteBNPL_PlanTypeAsync(int id);

        //Custom Query Operations
        Task<PaginationResultDto<BNPL_PlanType>> GetAllWithPaginationAsync(int pageNumber, int pageSize, string? searchKey = null);
    }
}