using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Repositories.IRepository
{
    public interface IBrandRepository
    {
        //CRUD operations
        Task<IEnumerable<Brand>> GetAllAsync();
        Task<Brand?> GetByIdAsync(int id);
        Task AddAsync(Brand brand);
        Task<Brand?> UpdateBrandAsync(int id, Brand brand);
        Task<bool> DeleteAsync(int id);

        //Custom Query Operations
        Task<PaginationResultDto<Brand>> GetAllWithPaginationAsync(int pageNumber, int pageSize, string? searchKey = null);
        Task<bool> ExistsByBrandNameAsync(string name);
        Task<bool> ExistsByBrandNameAsync(string name, int excludeId);
    }
}

