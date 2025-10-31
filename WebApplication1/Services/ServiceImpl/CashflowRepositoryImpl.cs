using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;

namespace WebApplication1.Services.ServiceImpl
{
    public class CashflowServiceImpl : ICashflowService
    {
        private readonly ICashflowRepository _repository;

        //logger: for auditing
        private readonly ILogger<CashflowServiceImpl> _logger;

        // Constructor
        public CashflowServiceImpl(ICashflowRepository repository, ILogger<CashflowServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _logger = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<Cashflow>> GetAllCashflowsAsync() =>
            await _repository.GetAllAsync();

        public Task<Cashflow?> GetCashflowByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task AddCashflowAsync(Cashflow cashflow)
        {
            throw new NotImplementedException();
        }

        public Task<Cashflow?> UpdateCashflowAsync(int id, Cashflow cashflow)
        {
            throw new NotImplementedException();
        }

        //Custom Query Operations
        public Task<PaginationResultDto<Cashflow>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? cashflowStatusId = null, string? searchKey = null)
        {
            throw new NotImplementedException();
        }
    }
}