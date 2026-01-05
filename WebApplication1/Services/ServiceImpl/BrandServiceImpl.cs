using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.Services.IService.Audit;
using WebApplication1.UOW.IUOW;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.ServiceImpl
{
    public class BrandServiceImpl : IBrandService
    {
        private readonly IBrandRepository _repository;
        private readonly IAppUnitOfWork _unitOfWork;

        private readonly IElectronicItemRepository _electronicItemRepository;

        // logger: for auditing
        // Audit Logging
        private readonly IAuditLogService _auditLogService;
        
        // Service-Level (Technical) Logging
        private readonly ILogger<BrandServiceImpl> _logger;

        // Constructor
        public BrandServiceImpl(IBrandRepository repository, IAppUnitOfWork unitOfWork, IElectronicItemRepository electronicItemRepository, IAuditLogService auditLogService, ILogger<BrandServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _unitOfWork = unitOfWork;
            _electronicItemRepository = electronicItemRepository;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<Brand>> GetAllBrandsAsync() =>
            await _repository.GetAllAsync();

        public async Task<Brand?> GetBrandByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        public async Task<Brand> AddBrandWithSaveAsync(Brand brand)
        {
            var duplicate = await _repository.ExistsByBrandNameAsync(brand.BrandName);
            if (duplicate)
                throw new Exception($"Brand with name '{brand.BrandName}' already exists.");

            await _repository.AddAsync(brand);
            await _unitOfWork.SaveChangesAsync();

            _auditLogService.LogEntityAction(AuditActionTypeEnum.Create, "Brand", brand.BrandID, brand.BrandName);
            return brand;
        }

        public async Task<Brand> UpdateBrandWithSaveAsync(int id, Brand brand)
        {
            var existingBrand = await _repository.GetByIdAsync(id);
            if (existingBrand == null)
                throw new Exception("Brand not found");

            var duplicate = await _repository.ExistsByBrandNameAsync(brand.BrandName, id);
            if (duplicate)
                throw new Exception($"Brand with name '{brand.BrandName}' already exists.");

            var updatedBrand = await _repository.UpdateAsync(id, brand);
            await _unitOfWork.SaveChangesAsync();

            if (updatedBrand == null)
                throw new Exception("Category update failed.");

            _auditLogService.LogEntityAction(AuditActionTypeEnum.Update, "Brand", updatedBrand.BrandID, updatedBrand.BrandName);
            return updatedBrand;
        }

        public async Task DeleteBrandWithSaveAsync(int id)
        {
            // Check if any ElectronicItems reference this brand
            bool hasItems = await _electronicItemRepository.ExistsByBrandAsync(id);
            if (hasItems)
            {
                _logger.LogWarning("Cannot delete brand {Id} — associated electronic items exist.", id);
                throw new InvalidOperationException("Cannot delete this brand because electronic items are associated with it.");
            }

            // Proceed with deletion if safe
            var deleted = await _repository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            if (!deleted)
                throw new KeyNotFoundException("Brand not found.");

            _auditLogService.LogEntityAction(AuditActionTypeEnum.Delete, "Brand", id, $"BrandId={id}");
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<Brand>> GetAllWithPaginationAsync(int pageNumber, int pageSize, string? searchKey = null)
        {
            return await _repository.GetAllWithPaginationAsync(pageNumber, pageSize, searchKey);
        }
    }
}
