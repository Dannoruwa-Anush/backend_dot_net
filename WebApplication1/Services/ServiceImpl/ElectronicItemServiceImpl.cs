using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;

namespace WebApplication1.Services.ServiceImpl
{
    public class ElectronicItemServiceImpl : IElectronicItemService
    {
        private readonly IElectronicItemRepository _repository;

        //logger: for auditing
        private readonly ILogger<ElectronicItemServiceImpl> _logger;

        // Constructor
        public ElectronicItemServiceImpl(IElectronicItemRepository repository, ILogger<ElectronicItemServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _logger = logger;
        }

        //Basic CRUD
        public async Task<IEnumerable<ElectronicItem>> GetAllElectronicItemsAsync() =>
            await _repository.GetAllAsync();


        public async Task<ElectronicItem?> GetElectronicItemByIdAsync(int id)=>
            await _repository.GetByIdAsync(id);
        
        public async Task<ElectronicItem> AddElectronicItemAsync(ElectronicItem electronicItem)
        {
            var duplicate = await _repository.ExistsByNameAsync(electronicItem.E_ItemName);
            if (duplicate)
                throw new Exception($"Electronic item with name '{electronicItem.E_ItemName}' already exists.");

            await _repository.AddAsync(electronicItem);

            _logger.LogInformation("Electronic item created: Id={Id}, Name={Name}", electronicItem.E_ItemID, electronicItem.E_ItemName);
            return electronicItem;
        }

        public async Task<ElectronicItem> UpdateElectronicItemAsync(int id, ElectronicItem electronicItem)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("Electronic item not found");

            var duplicate = await _repository.ExistsByNameAsync(electronicItem.E_ItemName, id);
            if (duplicate)
                throw new Exception($"Electronic item with name '{electronicItem.E_ItemName}' already exists.");

            var updatedElectronicItem = await _repository.UpdateAsync(id, electronicItem);

            if (updatedElectronicItem != null)
            {
                _logger.LogInformation("Electronic item updated: Id={Id}, Name={Name}", updatedElectronicItem.E_ItemID, updatedElectronicItem.E_ItemName);
                return updatedElectronicItem;
            }

            throw new Exception("Electronic item update failed.");
        }

        public async Task DeleteElectronicItemAsync(int id)
        {
            var deleted = await _repository.DeleteAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("Attempted to delete electronic item with id {Id}, but it does not exist.", id);
                throw new Exception("Electronic item not found");
            }

            _logger.LogInformation("Electronic item deleted successfully: Id={Id}", id);
        }

        //Custom Quaries 
        public async Task<PaginationResultDto<ElectronicItem>> GetAllWithPaginationAsync(int pageNumber, int pageSize)
        {
            return await _repository.GetAllWithPaginationAsync(pageNumber, pageSize);
        }

        public async Task<IEnumerable<ElectronicItem>> GetAllAllElectronicItemsByCategoryIdAsync(int categoryId)=>
            await _repository.GetAllByCategoryAsync(categoryId);


        public async Task<IEnumerable<ElectronicItem>> GetAllAllElectronicItemsByBrandIdAsync(int brandId)=>
            await _repository.GetAllByBrandAsync(brandId);
    }
}