using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Services.IService
{
    public interface IBrandService
    {
        //CRUD operations
        Task<IEnumerable<Brand>> GetAllBrandsAsync();
        Task<Brand?> GetBrandByIdAsync(int id);
        Task<Brand> AddBrandAsync(Brand brand);
        Task<Brand> UpdateBrandAsync(int id, Brand brand);
        Task DeleteBrandAsync(int id);

        //Custom Query Operations
        Task<PaginationResultDto<Brand>> GetAllWithPaginationAsync(int pageNumber, int pageSize);
    }
}
