using WebApplication1.Models;
using WebApplication1.Utils;

namespace WebApplication1.Services.IService
{
    public interface IBrandService
    {
        Task<IEnumerable<Brand>> GetAllBrandsAsync();
        Task<Brand?> GetBrandByIdAsync(int id);
        Task<Brand> AddBrandAsync(Brand brand);
        Task<Brand> UpdateBrandAsync(int id, Brand brand);
        Task DeleteBrandAsync(int id);
        Task<PaginationResult<Brand>> GetAllWithPaginationAsync(int pageNumber, int pageSize);
    }
}
