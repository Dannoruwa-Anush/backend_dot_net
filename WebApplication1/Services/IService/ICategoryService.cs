using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Services.IService
{
    public interface ICategoryService
    {
        //CRUD operations
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int id);
        Task<Category> AddCategoryAsync(Category category);
        Task<Category> UpdateCategoryAsync(int id, Category category);
        Task DeleteCategoryAsync(int id);

        //Custom Query Operations
        Task<PaginationResultDto<Category>> GetAllWithPaginationAsync(int pageNumber, int pageSize, string? searchKey = null);
    }
}