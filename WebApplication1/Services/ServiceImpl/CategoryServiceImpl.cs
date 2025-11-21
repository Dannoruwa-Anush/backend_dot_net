using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.UOW.IUOW;

namespace WebApplication1.Services.ServiceImpl
{
    public class CategoryServiceImpl : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly IAppUnitOfWork _unitOfWork;
        
        private readonly IElectronicItemRepository _electronicItemRepository;

        //logger: for auditing
        private readonly ILogger<CategoryServiceImpl> _logger;

        // Constructor
        public CategoryServiceImpl(ICategoryRepository repository, IAppUnitOfWork unitOfWork, IElectronicItemRepository electronicItemRepository, ILogger<CategoryServiceImpl> logger)
        {
            // Dependency injection
            _repository               = repository;
            _unitOfWork               = unitOfWork;
            _electronicItemRepository = electronicItemRepository;
            _logger                   = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync() =>
            await _repository.GetAllAsync();

        public async Task<Category?> GetCategoryByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        public async Task<Category> AddCategoryWithSaveAsync(Category category)
        {
            var dupliacte = await _repository.ExistsByCategoryNameAsync(category.CategoryName);
            if (dupliacte)
                throw new Exception($"Category with name '{category.CategoryName} already exists.");

            await _repository.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Category created: Id={Id}, CategoryName={Name}", category.CategoryID, category.CategoryName);
            return category;
        }

        public async Task<Category> UpdateCategoryWithSaveAsync(int id, Category category)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) 
                throw new Exception("Category not found");

            var duplicate = await _repository.ExistsByCategoryNameAsync(category.CategoryName, id);
            if (duplicate)
                throw new Exception($"Category with name '{category.CategoryName}' already exists.");

            var updatedCategory = await _repository.UpdateAsync(id, category);
            await _unitOfWork.SaveChangesAsync();

            if (updatedCategory != null)
            {
                _logger.LogInformation("Category updated: Id={Id}, CategoryName={Name}", updatedCategory.CategoryID, updatedCategory.CategoryName);
                return updatedCategory;
            }

            throw new Exception("Category update failed.");
        }

        public async Task DeleteCategoryWithSaveAsync(int id)
        {
            // Check if any ElectronicItems reference this category
            bool hasItems = await _electronicItemRepository.ExistsByCategoryAsync(id);
            if (hasItems)
            {
                _logger.LogWarning("Cannot delete category {Id} — associated electronic items exist.", id);
                throw new InvalidOperationException("Cannot delete this categoey because electronic items are associated with it.");
            }

            // Proceed with deletion if safe
            var deleted = await _repository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            
            if (!deleted)
            {
                _logger.LogWarning("Attempted to delete category with id {Id}, but it does not exist.", id);
                throw new Exception("Category not found");
            }

            _logger.LogInformation("Category deleted successfully: Id={Id}", id);
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<Category>> GetAllWithPaginationAsync(int pageNumber, int pageSize, string? searchKey = null)
        {
            return await _repository.GetAllWithPaginationAsync(pageNumber, pageSize, searchKey);
        }
    }
}