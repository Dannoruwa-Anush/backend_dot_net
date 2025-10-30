using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Repositories.IRepository
{
    public interface IElectronicItemRepository
    {
        //CRUD operations
        Task<IEnumerable<ElectronicItem>> GetAllAsync();
        Task<ElectronicItem?> GetByIdAsync(int id);
        Task AddAsync(ElectronicItem electronicItem);
        Task<ElectronicItem?> UpdateAsync(int id, ElectronicItem electronicItem);
        Task<bool> DeleteAsync(int id);

        //Custom Query Operations
        Task<PaginationResultDto<ElectronicItem>> GetAllWithPaginationAsync(int pageNumber, int pageSize);
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> ExistsByNameAsync(string name, int excludeId);
        Task<IEnumerable<ElectronicItem>> GetAllByCategoryAsync(int categoryId);
        Task<IEnumerable<ElectronicItem>> GetAllByBrandAsync(int brandId);
        Task<bool> ExistsByCategoryAsync(int categoryId);
        Task<bool> ExistsByBrandAsync(int brandId);
    }
}