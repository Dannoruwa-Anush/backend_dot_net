using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.Services.IService.Audit;
using WebApplication1.UOW.IUOW;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.ServiceImpl
{
    public class ElectronicItemServiceImpl : IElectronicItemService
    {
        private readonly IElectronicItemRepository _repository;
        private readonly IAppUnitOfWork _unitOfWork;

        //logger: for auditing
        // Audit Logging
        private readonly IAuditLogService _auditLogService;

        // Service-Level (Technical) Logging
        private readonly ILogger<ElectronicItemServiceImpl> _logger;

        // Constructor
        public ElectronicItemServiceImpl(IElectronicItemRepository repository, IAppUnitOfWork unitOfWork, IAuditLogService auditLogService, ILogger<ElectronicItemServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<ElectronicItem>> GetAllElectronicItemsAsync() =>
            await _repository.GetAllWithBrandCategoryDetailsAsync();


        public async Task<ElectronicItem?> GetElectronicItemByIdAsync(int id) =>
            await _repository.GetWithBrandCategoryDetailsByIdAsync(id);

        public async Task<ElectronicItem> AddElectronicItemWithSaveAsync(ElectronicItem electronicItem)
        {
            var duplicate = await _repository.ExistsByNameAsync(electronicItem.ElectronicItemName);
            if (duplicate)
                throw new Exception($"Electronic item with name '{electronicItem.ElectronicItemName}' already exists.");

            await _repository.AddAsync(electronicItem);
            await _unitOfWork.SaveChangesAsync();

            _auditLogService.LogEntityAction(AuditActionTypeEnum.Create, "Electronic item", electronicItem.ElectronicItemID, electronicItem.ElectronicItemName);
            return electronicItem;
        }

        public async Task<ElectronicItem> UpdateElectronicItemWithSaveAsync(int id, ElectronicItem electronicItem)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("Electronic item not found");

            var duplicate = await _repository.ExistsByNameAsync(electronicItem.ElectronicItemName, id);
            if (duplicate)
                throw new Exception($"Electronic item with name '{electronicItem.ElectronicItemName}' already exists.");

            var updatedElectronicItem = await _repository.UpdateAsync(id, electronicItem);
            await _unitOfWork.SaveChangesAsync();

            if (updatedElectronicItem == null)
                throw new Exception("Electronic item update failed.");

            _auditLogService.LogEntityAction(AuditActionTypeEnum.Update, "Electronic item", updatedElectronicItem.ElectronicItemID, updatedElectronicItem.ElectronicItemName);
            return updatedElectronicItem;
        }

        public async Task DeleteElectronicItemWithSaveAsync(int id)
        {
            var deleted = await _repository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            if (!deleted)
                throw new Exception("Electronic item not found");

            _auditLogService.LogEntityAction(AuditActionTypeEnum.Delete, "Electronic item", id, $"ElectronicItemId={id}");
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<ElectronicItem>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? categoryId = null, int? brandId = null, string? searchKey = null) =>
            await _repository.GetAllWithPaginationAsync(pageNumber, pageSize, categoryId, brandId, searchKey);

        public async Task<IEnumerable<ElectronicItem>> GetAllElectronicItemsByCategoryIdAsync(int categoryId) =>
            await _repository.GetAllByCategoryAsync(categoryId);


        public async Task<IEnumerable<ElectronicItem>> GetAllElectronicItemsByBrandIdAsync(int brandId) =>
            await _repository.GetAllByBrandAsync(brandId);

        public async Task<List<ElectronicItem>> GetAllElectronicItemsByIdsAsync(List<int> ids) =>
            await _repository.GetAllByIdsAsync(ids);
    }
}