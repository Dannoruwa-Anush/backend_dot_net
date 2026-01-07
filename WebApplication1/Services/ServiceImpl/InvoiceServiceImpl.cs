using WebApplication1.DTOs.RequestDto;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.UOW.IUOW;
using WebApplication1.Utils.Project_Enums;

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
        public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync() =>
            await _repository.GetAllAsync();

        public async Task<Invoice?> GetInvoiceByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        public Task<Invoice> BuildInvoiceAddRequestAsync(CustomerOrder order, CustomerOrderRequestDto request)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            // ============================
            // FULL PAYMENT ORDER
            // ============================
            if (!request.Bnpl_PlanTypeID.HasValue)
            {
                return Task.FromResult(new Invoice
                {
                    OrderID = order.OrderID,
                    InvoiceAmount = order.TotalAmount,
                    InvoiceType = InvoiceTypeEnum.Full_Payment_Invoice,
                    InvoiceStatus = InvoiceStatusEnum.Unpaid
                });
            }

            // ============================
            // BNPL - INITIAL PAYMENT ONLY
            // ============================
            return Task.FromResult(new Invoice
            {
                OrderID = order.OrderID,
                InvoiceAmount = request.InitialPayment!.Value,
                InvoiceType = InvoiceTypeEnum.Bnpl_Initial_Payment_Invoice,
                InvoiceStatus = InvoiceStatusEnum.Unpaid,
                InstallmentNo = 0
            });
        }

        public async Task<Invoice> CreateInstallmentInvoiceAsync(CustomerOrder order, int installmentNo)
        {
            decimal remaining =
                order.TotalAmount - order.Invoices
                    .Where(i => i.InvoiceType ==
                        InvoiceTypeEnum.Bnpl_Initial_Payment_Invoice)
                    .Sum(i => i.InvoiceAmount);

            int totalInstallments = order.BNPL_PLAN!.Bnpl_TotalInstallmentCount;

            decimal installmentAmount =
                Math.Round(remaining / totalInstallments, 2);

            var invoice = new Invoice
            {
                OrderID = order.OrderID,
                InvoiceAmount = installmentAmount,
                InvoiceType = InvoiceTypeEnum.Bnpl_Installment_Payment_Invoice,
                InvoiceStatus = InvoiceStatusEnum.Unpaid,
                InstallmentNo = installmentNo
            };

            order.Invoices.Add(invoice);
            await _unitOfWork.SaveChangesAsync();

            return invoice;
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<Invoice>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? invoiceTypeId = null, int? invoiceStatusId = null, string? searchKey = null)
        {
            return await _repository.GetAllWithPaginationAsync(pageNumber, pageSize, invoiceTypeId, invoiceStatusId, searchKey);
        }
    }
}