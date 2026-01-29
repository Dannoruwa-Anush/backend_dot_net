using System.Security;
using System.Text.Json;
using WebApplication1.DTOs.RequestDto.Payment;
using WebApplication1.DTOs.ResponseDto.BnplSnapshotPayingSimulation;
using WebApplication1.Models;
using WebApplication1.Services.IService;
using WebApplication1.Services.IService.Helper;
using WebApplication1.UOW.IUOW;
using WebApplication1.Utils.Helpers;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.ServiceImpl.Helper
{
    public class PaymentServiceImpl : IPaymentService
    {
        private readonly IAppUnitOfWork _unitOfWork;

        private readonly ICustomerOrderService _customerOrderService;
        private readonly ICashflowService _cashflowService;
        private readonly IBNPL_InstallmentService _bNPL_InstallmentService;
        private readonly IBNPL_PlanSettlementSummaryService _bnpl_planSettlementSummaryService;
        private readonly IInvoiceService _invoiceService;

        //logger: for auditing
        private readonly ILogger<PaymentServiceImpl> _logger;

        // Constructor
        public PaymentServiceImpl(
        IAppUnitOfWork unitOfWork,

        ICustomerOrderService customerOrderService,
        ICashflowService cashflowService,
        IBNPL_InstallmentService bNPL_InstallmentService,
        IBNPL_PlanSettlementSummaryService bnpl_planSettlementSummaryService,
        IInvoiceService invoiceService,
        ILogger<PaymentServiceImpl> logger)
        {
            // Dependency injection
            _unitOfWork = unitOfWork;

            _customerOrderService = customerOrderService;
            _cashflowService = cashflowService;
            _bNPL_InstallmentService = bNPL_InstallmentService;
            _bnpl_planSettlementSummaryService = bnpl_planSettlementSummaryService;
            _invoiceService = invoiceService;
            _logger = logger;
        }

        public async Task<Invoice> ProcessPaymentAsync(PaymentRequestDto paymentRequest)
        {
            var invoice = await _invoiceService.GetInvoiceWithOrderFinancialDetailsAsync(paymentRequest.InvoiceId)
                ?? throw new Exception("Invoice not found");

            if (invoice.InvoiceStatus == InvoiceStatusEnum.Paid)
                throw new InvalidOperationException("Invoice already paid");

            var order = invoice.CustomerOrder
                ?? throw new Exception("Order not loaded");

            ValidateBeforePayment(order);

            await _unitOfWork.BeginTransactionAsync();
            Cashflow newCashflow;
            try
            {
                newCashflow = invoice.InvoiceType switch
                {
                    InvoiceTypeEnum.Full_Pay => await ProcessFullPaymentAsync(order, invoice, paymentRequest),
                    InvoiceTypeEnum.Bnpl_Initial_Pay => await ProcessInitialBnplPaymentAsync(order, invoice, paymentRequest),
                    InvoiceTypeEnum.Bnpl_Installment_Pay => await ProcessBnplInstallmentPaymentAsync(order, invoice, paymentRequest),
                    _ => throw new InvalidOperationException("Unsupported invoice type")
                };

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            // Side-effect AFTER payment commit
            await _cashflowService.GenerateCashflowReceiptAsync(newCashflow.CashflowID);

            return invoice;
        }

        //Helper : ProcessFullPaymentAsync
        private async Task<Cashflow> ProcessFullPaymentAsync(CustomerOrder order, Invoice invoice, PaymentRequestDto paymentRequest)
        {
            order.OrderStatus = OrderStatusEnum.Processing;
            order.OrderPaymentStatus = OrderPaymentStatusEnum.Fully_Paid;

            invoice.InvoiceStatus = InvoiceStatusEnum.Paid;
            invoice.PaidAt = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            var cashflow = await _cashflowService
                .BuildCashflowAddRequestAsync(paymentRequest, CashflowTypeEnum.FullPayment);

            invoice.Cashflows.Add(cashflow);

            return cashflow;
        }

        //Helper : ProcessInitialBnplPaymentAsync
        private async Task<Cashflow> ProcessInitialBnplPaymentAsync(CustomerOrder order, Invoice invoice, PaymentRequestDto paymentRequest)
        {
            invoice.InvoiceStatus = InvoiceStatusEnum.Paid;
            invoice.PaidAt = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            var cashflow = await _cashflowService
                .BuildCashflowAddRequestAsync(paymentRequest, CashflowTypeEnum.BnplInitialPayment);

            invoice.Cashflows.Add(cashflow);

            order.BNPL_PLAN!.Bnpl_Status = BnplStatusEnum.Active;
            order.OrderStatus = OrderStatusEnum.Processing;
            order.OrderPaymentStatus = OrderPaymentStatusEnum.Partially_Paid;

            return cashflow;
        }

        //Helper : ProcessBnplInstallmentPaymentAsync
        private async Task<Cashflow> ProcessBnplInstallmentPaymentAsync(CustomerOrder order, Invoice invoice, PaymentRequestDto paymentRequest)
        {
            if (invoice.InvoiceStatus == InvoiceStatusEnum.Paid)
                throw new InvalidOperationException("Invoice already settled");

            if (string.IsNullOrWhiteSpace(invoice.SettlementSnapshotJson) || string.IsNullOrWhiteSpace(invoice.SettlementSnapshotHash))
                throw new SecurityException("Missing settlement snapshot or hash");

            var plan = order.BNPL_PLAN
                ?? throw new InvalidOperationException("BNPL plan not loaded");

            if (!plan.BNPL_PlanSettlementSummaries.Any())
                throw new InvalidOperationException("Settlement summaries not loaded");

            if (!plan.BNPL_Installments.Any())
                throw new InvalidOperationException("Installments not loaded");

            // Deserialize frozen snapshot
            var frozenSnapshot = JsonSerializer.Deserialize<BnplLatestSnapshotSettledResultDto>(invoice.SettlementSnapshotJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? throw new Exception("Invalid settlement snapshot");

            // Integrity verification (IMMUTABLE)
            var canonicalJson = SnapshotHashHelper.SerializeCanonical(JsonDocument.Parse(invoice.SettlementSnapshotJson).RootElement);

            var computedHash = SnapshotHashHelper.BuildHash(canonicalJson);

            if (!string.Equals(computedHash, invoice.SettlementSnapshotHash, StringComparison.Ordinal))
                throw new SecurityException("Settlement snapshot integrity violation");

            _bnpl_planSettlementSummaryService.ApplyFrozenSettlementSnapshot(order, frozenSnapshot);
            _bNPL_InstallmentService.BuildBnplInstallmentSettlement(order, frozenSnapshot);

            var cashflow = await _cashflowService.BuildCashflowAddRequestAsync(paymentRequest, CashflowTypeEnum.BnplInstallmentPayment);

            invoice.Cashflows.Add(cashflow);

            invoice.InvoiceStatus = InvoiceStatusEnum.Paid;
            invoice.PaidAt = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            if (plan.Bnpl_RemainingInstallmentCount == 0)
            {
                plan.Bnpl_Status = BnplStatusEnum.Completed;
                plan.CompletedAt = invoice.PaidAt;

                order.OrderPaymentStatus = OrderPaymentStatusEnum.Fully_Paid;
            }
            else
            {
                order.OrderPaymentStatus = OrderPaymentStatusEnum.Partially_Paid;
            }

            return cashflow;
        }

        //Helper : to valiadate payment
        private void ValidateBeforePayment(CustomerOrder order)
        {
            switch (order.OrderStatus)
            {
                // These states allow payments
                case OrderStatusEnum.Pending:
                case OrderStatusEnum.Processing:
                case OrderStatusEnum.Shipped:
                case OrderStatusEnum.Delivered:
                case OrderStatusEnum.DeliveredAfterCancellationRejected:
                    break;

                case OrderStatusEnum.Cancel_Pending:
                    throw new InvalidOperationException(
                        "Payment cannot be processed while a cancellation request is pending.");

                case OrderStatusEnum.Cancelled:
                    throw new InvalidOperationException(
                        "Payment cannot be processed for cancelled orders.");

                default:
                    throw new InvalidOperationException("Invalid order status.");
            }

            // -------------------------
            // BNPL Plan Validation
            // -------------------------
            var bnplPlan = order.BNPL_PLAN;

            // Full payment (no BNPL plan)
            if (bnplPlan == null)
                return;

            // BNPL payments
            if (bnplPlan.Bnpl_Status == BnplStatusEnum.Completed)
                throw new InvalidOperationException("BNPL plan is already completed. No additional payments are allowed.");

            if (bnplPlan.Bnpl_RemainingInstallmentCount <= 0)
                throw new InvalidOperationException("No remaining installments exist for this BNPL plan.");
        }
    }
}