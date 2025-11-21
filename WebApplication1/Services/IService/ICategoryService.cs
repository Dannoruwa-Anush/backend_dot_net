using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Services.IService
{
    public interface ICategoryService
    {
        //CRUD operations
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int id);

        //Single Repository Operations (save immediately)
        Task<Category> AddCategoryWithSaveAsync(Category category);
        Task<Category> UpdateCategoryWithSaveAsync(int id, Category category);
        Task DeleteCategoryWithSaveAsync(int id);

        //Custom Query Operations
        Task<PaginationResultDto<Category>> GetAllWithPaginationAsync(int pageNumber, int pageSize, string? searchKey = null);
    }
}