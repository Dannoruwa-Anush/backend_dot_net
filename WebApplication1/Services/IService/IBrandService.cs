using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Services.IService
{
    public interface IBrandService
    {
        //CRUD operations
        Task<IEnumerable<Brand>> GetAllBrandsAsync();
        Task<Brand?> GetBrandByIdAsync(int id);

        //Single Repository Operations (save immediately)
        Task<Brand> AddBrandWithSaveAsync(Brand brand);
        Task<Brand> UpdateBrandWithSaveAsync(int id, Brand brand);
        Task DeleteBrandWithSaveAsync(int id);

        //Custom Query Operations
        Task<PaginationResultDto<Brand>> GetAllWithPaginationAsync(int pageNumber, int pageSize, string? searchKey = null);
    }
}
