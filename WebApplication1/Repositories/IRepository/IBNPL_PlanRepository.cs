using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Repositories.IRepository
{
    public interface IBNPL_PlanRepository
    {
        //CRUD operations
        Task<IEnumerable<BNPL_PLAN>> GetAllAsync();
        Task<BNPL_PLAN?> GetByIdAsync(int id);
        Task AddAsync(BNPL_PLAN bNPL_Plan);
        Task<BNPL_PLAN?> UpdateAsync(int id, BNPL_PLAN bNPL_Plan);

        //Custom Query Operations
        Task<PaginationResultDto<BNPL_PLAN>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? planStatusId = null, string? searchKey = null);
        Task<bool> ExistsByBnplPlanTypeAsync(int bnplPlanTypeId);
    }
}