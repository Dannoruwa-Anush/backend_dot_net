using System.Text.Json;
using WebApplication1.DTOs.RequestDto.BnplSnapshotPayingSimulation;
using WebApplication1.DTOs.ResponseDto.BnplSnapshotPayingSimulation;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.UOW.IUOW;
using WebApplication1.Utils.Helpers;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.ServiceImpl
{
    public class InvoiceServiceImpl : IInvoiceService
    {
        private readonly IInvoiceRepository _repository;
        private readonly IAppUnitOfWork _unitOfWork;

        private readonly ICustomerOrderRepository _customerOrderRepository;
        private IDocumentGenerationService _documentGenerationService;
        private readonly IBNPL_PlanSettlementSummaryService _bnpl_planSettlementSummaryService;

        //logger: for auditing
        private readonly ILogger<InvoiceServiceImpl> _logger;

        // Constructor
        public InvoiceServiceImpl(IInvoiceRepository repository, IAppUnitOfWork unitOfWork, ICustomerOrderRepository customerOrderRepository, IDocumentGenerationService documentGenerationService, IBNPL_PlanSettlementSummaryService bnpl_planSettlementSummaryService, ILogger<InvoiceServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _unitOfWork = unitOfWork;
            _customerOrderRepository = customerOrderRepository;
            _documentGenerationService = documentGenerationService;
            _bnpl_planSettlementSummaryService = bnpl_planSettlementSummaryService;
            _logger = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync() =>
            await _repository.GetAllAsync();

        public async Task<Invoice?> GetInvoiceByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        public async Task<Invoice?> GetInvoiceWithOrderAsync(int id) =>
            await _repository.GetInvoiceWithOrderAsync(id);    

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

        // Helper method: attach invoice
        private async Task GenerateAndAttachInvoicePdfAsync(CustomerOrder order, Invoice invoice)
        {
            var fileUrl = await _documentGenerationService.GenerateInvoicePdfAsync(order, invoice);

            invoice.InvoiceFileUrl = fileUrl;

            await _unitOfWork.SaveChangesAsync();
        }
        // ----------- [End : Invoice Generation] -------------

        //Custom Query Operations
        public async Task<PaginationResultDto<Invoice>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? invoiceTypeId = null, int? invoiceStatusId = null, int? customerId = null, int? orderSourceId = null, string? searchKey = null)
        {
            return await _repository.GetAllWithPaginationAsync(pageNumber, pageSize, invoiceTypeId, invoiceStatusId, customerId, orderSourceId, searchKey);
        }

        public async Task<bool> ExistsUnpaidInvoiceByCustomerAsync(int customerId) =>
            await _repository.ExistsUnpaidInvoiceByCustomerAsync(customerId);

        public async Task<Invoice> GenerateInvoiceForSettlementSimulationAsync(BnplSnapshotPayingSimulationRequestDto request)
        {
            var order = await _customerOrderRepository.GetWithFinancialDetailsByIdAsync(request.OrderId)
                ?? throw new Exception("Order not found");

            // Prevent double billing
            if (order.Invoices.Any(i =>
                    i.InvoiceType == InvoiceTypeEnum.Bnpl_Installment_Payment &&
                    i.InvoiceStatus == InvoiceStatusEnum.Unpaid))
                throw new InvalidOperationException("Existing unpaid installment invoice found");

            // 1. Run simulation
            var simulation = await _bnpl_planSettlementSummaryService.SimulateBnplPlanSettlementAsync(request);

            // 2. Convert simulation -> settlement snapshot (IMPORTANT)
            var frozenSnapshot = new BnplLatestSnapshotSettledResultDto
            {
                TotalPaidArrears = simulation.PaidToArrears,
                TotalPaidLateInterest = simulation.PaidToInterest,
                TotalPaidCurrentInstallmentBase = simulation.PaidToBase,
                OverPaymentCarriedToNextInstallment = simulation.OverPaymentCarried
            };

            // 3. Canonical serialization
            var snapshotJson = SnapshotHashHelper.SerializeCanonical(frozenSnapshot);

            // 4. Hash (IMMUTABLE INTEGRITY SEAL)
            var snapshotHash = SnapshotHashHelper.BuildHash(snapshotJson);

            // 5. Create invoice with frozen snapshot
            var invoice = new Invoice
            {
                OrderID = order.OrderID,
                InvoiceType = InvoiceTypeEnum.Bnpl_Installment_Payment,
                InvoiceStatus = InvoiceStatusEnum.Unpaid,
                InvoiceAmount = request.PaymentAmount,
                InstallmentNo = simulation.InstallmentId,

                SettlementSnapshotJson = snapshotJson,
                SettlementSnapshotHash = snapshotHash
            };

            order.Invoices.Add(invoice);
            await _unitOfWork.SaveChangesAsync();

            await GenerateAndAttachInvoicePdfAsync(order, invoice);

            return invoice;
        }

        public async Task GenerateReceiptAsync(int invoiceId)
        {
            var invoice = await _repository.GetInvoiceWithOrderAsync(invoiceId)
                ?? throw new Exception("Invoice not found");

            if (invoice.InvoiceStatus != InvoiceStatusEnum.Paid)
                throw new InvalidOperationException("Receipt allowed only for PAID invoices");

            // Idempotency guard
            if (!string.IsNullOrEmpty(invoice.ReceiptFileUrl))
                return;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Re-check inside transaction
                if (!string.IsNullOrEmpty(invoice.ReceiptFileUrl))
                {
                    await _unitOfWork.RollbackAsync();
                    return;
                }

                var receiptUrl =
                    await _documentGenerationService.GenerateReceiptPdfAsync(
                        invoice.CustomerOrder!, invoice);

                invoice.ReceiptFileUrl = receiptUrl;

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}