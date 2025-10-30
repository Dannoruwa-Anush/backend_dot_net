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
        Task<PaginationResultDto<ElectronicItem>> GetAllWithPaginationAsync(int pageNumber, int pageSize);

        //Helping operations
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> ExistsByNameAsync(string name, int excludeId);
    }
}