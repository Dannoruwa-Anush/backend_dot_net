using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;

namespace WebApplication1.Services.ServiceImpl
{
    public class BNPL_PlanTypeServiceImpl : IBNPL_PlanTypeService
    {
        private readonly IBNPL_PlanTypeRepository _repository;

        private readonly IBNPL_PlanRepository _bnpl_planRepository;

        //logger: for auditing
        private readonly ILogger<BNPL_PlanTypeServiceImpl> _logger;

        // Constructor
        public BNPL_PlanTypeServiceImpl(IBNPL_PlanTypeRepository repository, IBNPL_PlanRepository bnpl_planRepository, ILogger<BNPL_PlanTypeServiceImpl> logger)
        {
            // Dependency injection
            _repository          = repository;
            _bnpl_planRepository = bnpl_planRepository;
            _logger              = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<BNPL_PlanType>> GetAllBNPL_PlanTypesAsync() =>
            await _repository.GetAllAsync();

        public async Task<BNPL_PlanType?> GetBNPL_PlanTypeByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        public async Task<BNPL_PlanType> AddBNPL_PlanTypeAsync(BNPL_PlanType bNPL_PlanType)
        {
            var duplicate = await _repository.ExistsByBNPL_PlanTypeNameAsync(bNPL_PlanType.Bnpl_PlanTypeName);
            if (duplicate)
                throw new Exception($"BNPL Plan Type with name '{bNPL_PlanType.Bnpl_PlanTypeName}' already exists.");

            await _repository.AddAsync(bNPL_PlanType);

            _logger.LogInformation("BNPL Plan Type created: Id={Id}, Name={Name}", bNPL_PlanType.Bnpl_PlanTypeID, bNPL_PlanType.Bnpl_PlanTypeName);
            return bNPL_PlanType;
        }
        
        public async Task<BNPL_PlanType> UpdateBNPL_PlanTypeAsync(int id, BNPL_PlanType bNPL_PlanType)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("BNPL plan type not found");

            var duplicate = await _repository.ExistsByBNPL_PlanTypeNameAsync(bNPL_PlanType.Bnpl_PlanTypeName, id);
            if (duplicate)
                throw new Exception($"BNPL plan type with email '{bNPL_PlanType.Bnpl_PlanTypeName}' already exists.");

            var updatedBNPL_PlanType = await _repository.UpdateAsync(id, bNPL_PlanType);

            if (updatedBNPL_PlanType != null)
            {
                _logger.LogInformation("BNPL Plan Type updated: Id={Id}, Name={Name}", updatedBNPL_PlanType.Bnpl_PlanTypeID, updatedBNPL_PlanType.Bnpl_PlanTypeName);
                return updatedBNPL_PlanType;
            }

            throw new Exception("BNPL plan type update failed.");
        }

        public async Task DeleteBNPL_PlanTypeAsync(int id)
        {
            // Check if any BNPL_Plans reference this BNPL_PlanType
            bool hasItems = await _bnpl_planRepository.ExistsByBnplPlanTypeAsync(id);
            if (hasItems)
            {
                _logger.LogWarning("Cannot delete BNPL plan type {Id} — associated BNPL plans exist.", id);
                throw new InvalidOperationException("Cannot delete this BNPL plan type because BNPL plan types are associated with it.");
            }

            // Proceed with deletion if safe
            var deleted = await _repository.DeleteAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("Attempted to delete BNPL  plan type with id {Id}, but it does not exist.", id);
                throw new Exception("BNPL plan type not found");
            }

            _logger.LogInformation("BNPL plan type deleted successfully: Id={Id}", id);
        }

        //CRUD operations
        public async Task<PaginationResultDto<BNPL_PlanType>> GetAllWithPaginationAsync(int pageNumber, int pageSize, string? searchKey = null)
        {
            return await _repository.GetAllWithPaginationAsync(pageNumber, pageSize, searchKey);
        }
    }
}