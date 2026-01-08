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

        private IDocumentGenerationService _documentGenerationService;
        //logger: for auditing
        private readonly ILogger<InvoiceServiceImpl> _logger;

        // Constructor
        public InvoiceServiceImpl(IInvoiceRepository repository, IAppUnitOfWork unitOfWork, IDocumentGenerationService documentGenerationService, ILogger<InvoiceServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _unitOfWork = unitOfWork;
            _documentGenerationService = documentGenerationService;
            _logger = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync() =>
            await _repository.GetAllAsync();

        public async Task<Invoice?> GetInvoiceByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        // ----------- [Start : Invoice Generation] -------------
        public async Task<Invoice> BuildInvoiceAddRequestAsync(CustomerOrder order, InvoiceTypeEnum invoiceType)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var invoice = invoiceType switch
            {
                InvoiceTypeEnum.Full_Payment =>
                    BuildFullPaymentInvoice(order),

                InvoiceTypeEnum.Bnpl_Initial_Payment =>
                    BuildBnplInitialInvoice(order),

                InvoiceTypeEnum.Bnpl_Installment_Payment =>
                    BuildInstallmentInvoice(order),

                _ => throw new InvalidOperationException("Unsupported invoice type")
            };

            order.Invoices.Add(invoice);
            await _unitOfWork.SaveChangesAsync();

            // Generate PDF AFTER InvoiceID exists
            await GenerateAndAttachInvoicePdfAsync(order, invoice);

            return invoice;
        }

        // Helper method: invoice builder - full payment
        private Invoice BuildFullPaymentInvoice(CustomerOrder order)
        {
            return new Invoice
            {
                OrderID = order.OrderID,
                InvoiceAmount = order.TotalAmount,
                InvoiceType = InvoiceTypeEnum.Full_Payment,
                InvoiceStatus = InvoiceStatusEnum.Unpaid
            };
        }

        // Helper method: invoice builder - bnpl initial payment
        private Invoice BuildBnplInitialInvoice(CustomerOrder order)
        {
            var plan = order.BNPL_PLAN
                ?? throw new InvalidOperationException("BNPL plan not found");

            return new Invoice
            {
                OrderID = order.OrderID,
                InvoiceAmount = plan.Bnpl_InitialPayment,
                InvoiceType = InvoiceTypeEnum.Bnpl_Initial_Payment,
                InvoiceStatus = InvoiceStatusEnum.Unpaid,
            };
        }

        // Helper method: invoice builder - bnpl installment payment
        private Invoice BuildInstallmentInvoice(CustomerOrder order)
        {
            var plan = order.BNPL_PLAN
                ?? throw new InvalidOperationException("BNPL plan not found");

            var latestSettlement = plan.BNPL_PlanSettlementSummaries
                .SingleOrDefault(s => s.IsLatest)
                ?? throw new InvalidOperationException("Latest settlement not found");

            return new Invoice
            {
                OrderID = order.OrderID,
                InvoiceAmount = latestSettlement.Total_PayableSettlement,
                InvoiceType = InvoiceTypeEnum.Bnpl_Installment_Payment,
                InvoiceStatus = InvoiceStatusEnum.Unpaid,
                InstallmentNo = latestSettlement.CurrentInstallmentNo
            };
        }

        // Helper method: attach invoice
        private async Task GenerateAndAttachInvoicePdfAsync(CustomerOrder order, Invoice invoice)
        {
            var fileUrl = await _documentGenerationService.GenerateInvoicePdfAsync(order, invoice);

            invoice.InvoiceFileUrl = fileUrl;

            await _unitOfWork.SaveChangesAsync();
        }
        // ----------- [End : Invoice Generation] -------------

        //Custom Query Operations
        public async Task<PaginationResultDto<Invoice>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? invoiceTypeId = null, int? invoiceStatusId = null, int? customerId = null, string? searchKey = null)
        {
            return await _repository.GetAllWithPaginationAsync(pageNumber, pageSize, invoiceTypeId, invoiceStatusId, customerId, searchKey);
        }
    }
}