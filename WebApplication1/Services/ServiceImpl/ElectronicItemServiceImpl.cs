using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.UOW.IUOW;

namespace WebApplication1.Services.ServiceImpl
{
    public class ElectronicItemServiceImpl : IElectronicItemService
    {
        private readonly IElectronicItemRepository _repository;
        private readonly IAppUnitOfWork _unitOfWork;

        //logger: for auditing
        private readonly ILogger<ElectronicItemServiceImpl> _logger;

        // Constructor
        public ElectronicItemServiceImpl(IElectronicItemRepository repository, IAppUnitOfWork unitOfWork, ILogger<ElectronicItemServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<ElectronicItem>> GetAllElectronicItemsAsync() =>
            await _repository.GetAllAsync();


        public async Task<ElectronicItem?> GetElectronicItemByIdAsync(int id)=>
            await _repository.GetByIdAsync(id);
        
        public async Task<ElectronicItem> AddElectronicItemAsync(ElectronicItem electronicItem)
        {
            var duplicate = await _repository.ExistsByNameAsync(electronicItem.ElectronicItemName);
            if (duplicate)
                throw new Exception($"Electronic item with name '{electronicItem.ElectronicItemName}' already exists.");

            await _repository.AddAsync(electronicItem);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Electronic item created: Id={Id}, Name={Name}", electronicItem.ElectronicItemID, electronicItem.ElectronicItemName);
            return electronicItem;
        }

        public async Task<ElectronicItem> UpdateElectronicItemAsync(int id, ElectronicItem electronicItem)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("Electronic item not found");

            var duplicate = await _repository.ExistsByNameAsync(electronicItem.ElectronicItemName, id);
            if (duplicate)
                throw new Exception($"Electronic item with name '{electronicItem.ElectronicItemName}' already exists.");

            var updatedElectronicItem = await _repository.UpdateAsync(id, electronicItem);
            await _unitOfWork.SaveChangesAsync();

            if (updatedElectronicItem != null)
            {
                _logger.LogInformation("Electronic item updated: Id={Id}, Name={Name}", updatedElectronicItem.ElectronicItemID, updatedElectronicItem.ElectronicItemName);
                return updatedElectronicItem;
            }

            throw new Exception("Electronic item update failed.");
        }

        public async Task DeleteElectronicItemAsync(int id)
        {
            var deleted = await _repository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            
            if (!deleted)
            {
                _logger.LogWarning("Attempted to delete electronic item with id {Id}, but it does not exist.", id);
                throw new Exception("Electronic item not found");
            }

            _logger.LogInformation("Electronic item deleted successfully: Id={Id}", id);
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<ElectronicItem>> GetAllWithPaginationAsync(int pageNumber, int pageSize, string? searchKey = null)
        {
            return await _repository.GetAllWithPaginationAsync(pageNumber, pageSize, searchKey);
        }

        public async Task<IEnumerable<ElectronicItem>> GetAllElectronicItemsByCategoryIdAsync(int categoryId)=>
            await _repository.GetAllByCategoryAsync(categoryId);


        public async Task<IEnumerable<ElectronicItem>> GetAllElectronicItemsByBrandIdAsync(int brandId)=>
            await _repository.GetAllByBrandAsync(brandId);
    }
}