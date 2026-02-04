using WebApplication1.DTOs.RequestDto.Payment;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.UOW.IUOW;
using WebApplication1.Utils.Helpers;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.ServiceImpl
{
    public class CashflowServiceImpl : ICashflowService
    {
        private readonly ICashflowRepository _repository;
        private readonly IAppUnitOfWork _unitOfWork;

        private readonly IInvoiceRepository _invoiceRepository;

        private IPhysicalShopSessionService _physicalShopSessionService;
        private IDocumentGenerationService _documentGenerationService;

        //logger: for auditing
        private readonly ILogger<CashflowServiceImpl> _logger;

        // Constructor
        public CashflowServiceImpl(ICashflowRepository repository, IAppUnitOfWork unitOfWork, IInvoiceRepository invoiceRepository, IPhysicalShopSessionService physicalShopSessionService, IDocumentGenerationService documentGenerationService, ILogger<CashflowServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _unitOfWork = unitOfWork;
            _invoiceRepository = invoiceRepository;
            _physicalShopSessionService = physicalShopSessionService;
            _documentGenerationService = documentGenerationService;
            _logger = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<Cashflow>> GetAllCashflowsAsync() =>
            await _repository.GetAllAsync();

        public async Task<Cashflow?> GetCashflowByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        //Custom Query Operations
        public async Task<PaginationResultDto<Cashflow>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? paymentNatureId = null, string? searchKey = null)
        {
            return await _repository.GetAllWithPaginationAsync(pageNumber, pageSize, paymentNatureId, searchKey);
        }

        public async Task<decimal> SumCashflowsByOrderAsync(int orderId) =>
            await _repository.SumCashflowsByOrderAsync(orderId);

        //Shared Internal Operations Used by Multiple Repositories
        public async Task<Cashflow> BuildCashflowAddRequestAsync(PaymentRequestDto paymentRequest, CashflowTypeEnum cashflowType)
        {
            if (paymentRequest == null)
                throw new ArgumentNullException(nameof(paymentRequest));

            var invoice = await _invoiceRepository.GetInvoiceWithOrderAsync(paymentRequest.InvoiceId);
            if (invoice == null)
                throw new Exception("Invoice not found");

            var order = invoice.CustomerOrder
                ?? throw new Exception("Order not loaded for this invoice");

            // Determine session based on order type and invoice type
            int? sessionId = null;
            if (order.OrderSource == OrderSourceEnum.PhysicalShop)
            {
                var activeSession = await _physicalShopSessionService.GetLatestActivePhysicalShopSessionAsync();

                switch (invoice.InvoiceType)
                {
                    case InvoiceTypeEnum.Full_Pay:
                    case InvoiceTypeEnum.Bnpl_Initial_Pay:
                        // Must match order session
                        if (!order.PhysicalShopSessionId.HasValue)
                            throw new InvalidOperationException("Physical shop order must have a session.");

                        if (activeSession == null || activeSession.PhysicalShopSessionID != order.PhysicalShopSessionId)
                            throw new InvalidOperationException("Active session does not match order session.");

                        sessionId = order.PhysicalShopSessionId;
                        break;

                    case InvoiceTypeEnum.Bnpl_Installment_Pay:
                        // Only require any active session if invoice channel is physical shop
                        if (invoice.InvoicePaymentChannel == InvoicePaymentChannelEnum.ByVisitingShop)
                        {
                            if (activeSession == null)
                                throw new InvalidOperationException("No active physical shop session for BNPL installment.");

                            sessionId = activeSession.PhysicalShopSessionID;
                        }
                        break;

                    default:
                        throw new InvalidOperationException("Unsupported invoice type for session validation.");
                }
            }

            // Determine status (default: Paid)
            var status = CashflowPaymentNatureEnum.Payment;

            var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            // Build reference
            var cashflowRef = $"CF-{paymentRequest.InvoiceId}-{status}-{cashflowType}-{now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..6]}";

            var newCashflow = new Cashflow
            {
                InvoiceID = paymentRequest.InvoiceId,
                AmountPaid = invoice.InvoiceAmount,
                CashflowDate = now,
                CashflowPaymentNature = status,
                CashflowRef = cashflowRef,
                PhysicalShopSessionId = sessionId,
            };

            var duplicate = await _repository.ExistsByCashflowRefAsync(newCashflow.CashflowRef);
            if (duplicate)
                throw new Exception($"Cash flow with ref '{newCashflow.CashflowRef}' already exists.");

            _logger.LogInformation("Generated Cashflow record: {CashflowRef}", newCashflow.CashflowRef);
            return newCashflow;
        }

        public async Task GenerateCashflowReceiptAsync(int cashflowId)
        {
            var cashflow = await _repository.GetCashflowWithInvoiceAsync(cashflowId)
                ?? throw new Exception("Cashflow not found");

            // Idempotency guard (per cashflow)
            if (cashflow.CashflowPaymentNature == CashflowPaymentNatureEnum.Payment &&
                !string.IsNullOrEmpty(cashflow.PaymentReceiptFileUrl))
                return;

            if (cashflow.CashflowPaymentNature == CashflowPaymentNatureEnum.Refund &&
                !string.IsNullOrEmpty(cashflow.RefundReceiptFileUrl))
                return;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                string receiptUrl;

                if (cashflow.CashflowPaymentNature == CashflowPaymentNatureEnum.Payment)
                {
                    receiptUrl = await _documentGenerationService.GeneratePaymentReceiptPdfAsync(
                        cashflow.Invoice!.CustomerOrder!, cashflow);

                    cashflow.PaymentReceiptFileUrl = receiptUrl;
                }
                else // Refund
                {
                    receiptUrl = await _documentGenerationService.GenerateRefundReceiptPdfAsync(
                        cashflow.Invoice!.CustomerOrder!, cashflow);

                    cashflow.RefundReceiptFileUrl = receiptUrl;
                }

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