using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;

namespace WebApplication1.Services.ServiceImpl
{
    public class BrandServiceImpl : IBrandService
    {
        private readonly IBrandRepository _repository;

        private readonly IElectronicItemRepository _electronicItemRepository;

        // logger: for auditing
        private readonly ILogger<BrandServiceImpl> _logger;

        // Constructor
        public BrandServiceImpl(IBrandRepository repository, IElectronicItemRepository electronicItemRepository, ILogger<BrandServiceImpl> logger)
        {
            // Dependency injection
            _repository               = repository;
            _electronicItemRepository = electronicItemRepository;
            _logger                   = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<Brand>> GetAllBrandsAsync() =>
            await _repository.GetAllAsync();

        public async Task<Brand?> GetBrandByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        public async Task<Brand> AddBrandAsync(Brand brand)
        {
            var duplicate = await _repository.ExistsByBrandNameAsync(brand.BrandName);
            if (duplicate)
                throw new Exception($"Brand with name '{brand.BrandName}' already exists.");

            await _repository.AddAsync(brand);

            _logger.LogInformation("Brand created: Id={Id}, BrandName={Name}", brand.BrandID, brand.BrandName);
            return brand;
        }

        //update: with transaction handling
        public async Task<Brand> UpdateBrandAsync(int id, Brand brand)
        {
            var existingBrand = await _repository.GetByIdAsync(id);
            if (existingBrand == null)
                throw new Exception("Brand not found");

            var duplicate = await _repository.ExistsByBrandNameAsync(brand.BrandName, id);
            if (duplicate)
                throw new Exception($"Brand with name '{brand.BrandName}' already exists.");

            var updatedBrand = await _repository.UpdateBrandWithTransactionAsync(id, brand);
            _logger.LogInformation("Brand updated: Id={Id}, Name={Name}", updatedBrand.BrandID, updatedBrand.BrandName);
            return updatedBrand;
        }

        public async Task DeleteBrandAsync(int id)
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
            if (!deleted)
            {
                _logger.LogWarning("Attempted to delete brand with id {Id}, but it does not exist.", id);
                throw new KeyNotFoundException("Brand not found.");
            }

            _logger.LogInformation("Brand deleted successfully: Id={Id}", id);
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<Brand>> GetAllWithPaginationAsync(int pageNumber, int pageSize)
        {
            return await _repository.GetAllWithPaginationAsync(pageNumber, pageSize);
        }
    }
}
