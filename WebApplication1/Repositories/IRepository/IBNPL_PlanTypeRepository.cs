using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Repositories.IRepository
{
    public interface IBNPL_PlanTypeRepository
    {
        //CRUD operations
        Task<IEnumerable<BNPL_PlanType>> GetAllAsync();
        Task<BNPL_PlanType?> GetByIdAsync(int id);
        Task AddAsync(BNPL_PlanType bNPL_PlanType);
        Task<BNPL_PlanType?> UpdateAsync(int id, BNPL_PlanType bNPL_PlanType);
        Task<bool> DeleteAsync(int id);
        Task<PaginationResultDto<BNPL_PlanType>> GetAllWithPaginationAsync(int pageNumber, int pageSize);

        //Helping operations
        Task<bool> ExistsByBNPL_PlanTypeNameAsync(string name);
        Task<bool> ExistsByBNPL_PlanTypeNameAsync(string name, int excludeId);
    }
}