using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Repositories.IRepository
{
    public interface IBNPL_PlanRepository
    {
        //CRUD operations
        Task<IEnumerable<BNPL_PLAN>> GetAllAsync();
        Task<IEnumerable<BNPL_PLAN>> GetAllWithBnplPlanAsync();
        Task<BNPL_PLAN?> GetByIdAsync(int id);
        Task<BNPL_PLAN?> GetWithPlanTypeCustomerDetailsByIdAsync(int id);
        Task AddAsync(BNPL_PLAN bNPL_Plan);
        //Note : Update will be handled by cancel order/ payment

        //Custom Query Operations
        Task<PaginationResultDto<BNPL_PLAN>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? planStatusId = null, string? searchKey = null);
        Task<bool> ExistsByBnplPlanTypeAsync(int bnplPlanTypeId);
        Task<BNPL_PLAN?> GetByOrderIdAsync(int OrderId);
        Task<IEnumerable<BNPL_PLAN>> GetAllActiveAsync();
    }
}