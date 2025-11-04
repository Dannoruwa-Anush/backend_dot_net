using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Repositories.IRepository
{
    public interface ICategoryRepository
    {
        //CRUD operations
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task AddAsync(Category category);
        Task<Category?> UpdateAsync(int id, Category category);
        Task<bool> DeleteAsync(int id);

        //Custom Query Operations
        Task<PaginationResultDto<Category>> GetAllWithPaginationAsync(int pageNumber, int pageSize, string? searchKey = null);
        Task<bool> ExistsByCategoryNameAsync(string name);
        Task<bool> ExistsByCategoryNameAsync(string name, int excludeId);

    }
}