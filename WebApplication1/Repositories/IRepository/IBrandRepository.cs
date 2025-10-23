using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

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
        Task<PaginationResultDto<Brand>> GetAllWithPaginationAsync(int pageNumber, int pageSize);
    }
}

