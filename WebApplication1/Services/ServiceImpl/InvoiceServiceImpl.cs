using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.UOW.IUOW;

namespace WebApplication1.Services.ServiceImpl
{
    public class InvoiceServiceImpl : IInvoiceService
    {
        private readonly IInvoiceRepository _repository;
        private readonly IAppUnitOfWork _unitOfWork;

        //logger: for auditing
        private readonly ILogger<InvoiceServiceImpl> _logger;

        // Constructor
        public InvoiceServiceImpl(IInvoiceRepository repository, IAppUnitOfWork unitOfWork, ILogger<InvoiceServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync()=>
            await _repository.GetAllAsync();

        public async Task<Invoice?> GetInvoiceByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        public Task<Invoice> BuildInvoiceAddRequestAsync()
        {
            throw new NotImplementedException();
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<Invoice>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? invoiceTypeId = null, int? invoiceStatusId = null, string? searchKey = null)
        {
            return await _repository.GetAllWithPaginationAsync(pageNumber, pageSize, invoiceTypeId, invoiceStatusId, searchKey);
        }
    }
}