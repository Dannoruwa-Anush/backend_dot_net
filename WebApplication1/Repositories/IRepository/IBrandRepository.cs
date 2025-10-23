using WebApplication1.Models;
using WebApplication1.Utils;

namespace WebApplication1.Repositories.IRepository
{
    public interface IBrandRepository
    {
        Task<IEnumerable<Brand>> GetAllAsync();
        Task<Brand?> GetByIdAsync(int id);
        Task AddAsync(Brand brand);
        Task<Brand?> UpdateBrandAsync(int id, Brand brand);
        Task<bool> DeleteAsync(int id);
        Task SaveAsync();
        Task<Brand> UpdateBrandWithTransactionAsync(int id, Brand brand);
        Task<PaginationResult<Brand>> GetAllWithPaginationAsync(int pageNumber, int pageSize);
    }
}

