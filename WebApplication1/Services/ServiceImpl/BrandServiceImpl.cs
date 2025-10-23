using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;

namespace WebApplication1.Services.ServiceImpl
{
    public class BrandServiceImpl : IBrandService
    {
        private readonly IBrandRepository _repository;

        // logger: for auditing
        private readonly ILogger<BrandServiceImpl> _logger;

        public BrandServiceImpl(IBrandRepository repository, ILogger<BrandServiceImpl> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<Brand>> GetAllBrandsAsync() =>
            await _repository.GetAllAsync();

        public async Task<Brand?> GetBrandByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        public async Task<Brand> AddBrandAsync(Brand brand)
        {
            var existing = await _repository.GetAllAsync();
            if (existing.Any(b => b.Name.ToLower() == brand.Name.ToLower()))
                throw new Exception($"Brand with name '{brand.Name}' already exists.");

            await _repository.AddAsync(brand);
            await _repository.SaveAsync();

            _logger.LogInformation("Brand created: Id={Id}, Name={Name}", brand.Id, brand.Name);
            return brand;
        }

        //update: with transaction handling
        public async Task<Brand> UpdateBrandAsync(int id, Brand brand)
        {
            var existingBrand = await _repository.GetByIdAsync(id);
            if (existingBrand == null) throw new Exception("Brand not found");

            var duplicate = (await _repository.GetAllAsync())
                            .Any(b => b.Name.ToLower() == brand.Name.ToLower() && b.Id != id);
            if (duplicate) throw new Exception($"Brand with name '{brand.Name}' already exists.");

            var updatedBrand = await _repository.UpdateBrandWithTransactionAsync(id, brand);
            _logger.LogInformation("Brand updated: Id={Id}, Name={Name}", updatedBrand.Id, updatedBrand.Name);
            return updatedBrand;
        }

        public async Task DeleteBrandAsync(int id)
        {
            var deleted = await _repository.DeleteAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("Attempted to delete brand with id {Id}, but it does not exist.", id);
                throw new Exception("Brand not found");
            }

            _logger.LogInformation("Brand deleted successfully: Id={Id}", id);
        }

        public async Task<PaginationResultDto<Brand>> GetAllWithPaginationAsync(int pageNumber, int pageSize)
        {
            return await _repository.GetAllWithPaginationAsync(pageNumber, pageSize);
        }
    }
}
