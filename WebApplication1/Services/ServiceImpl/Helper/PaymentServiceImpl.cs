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

        public async Task<bool> ProcessPaymentAsync(PaymentRequestDto paymentRequest)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(paymentRequest.InvoiceId)
                ?? throw new Exception("Invoice not found");

            if (invoice.InvoiceStatus == InvoiceStatusEnum.Paid)
                throw new Exception("Invoice already paid");

            if (paymentRequest.PaymentAmount <= 0)
                throw new Exception("Payment amount should be a positive number");

            var order = await _customerOrderService.GetCustomerOrderWithFinancialDetailsByIdAsync(invoice.OrderID)
                ?? throw new Exception("Order not found");

            if (paymentRequest.PaymentAmount != invoice.InvoiceAmount)
                throw new Exception("Payment amount must equal invoice amount");

            ValidateBeforePayment(order);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                switch (invoice.InvoiceType)
                {
                    case InvoiceTypeEnum.Full_Payment:
                        await ProcessFullPaymentAsync(order, invoice, paymentRequest);
                        break;

                    case InvoiceTypeEnum.Bnpl_Initial_Payment:
                        await ProcessInitialBnplPaymentAsync(order, invoice, paymentRequest);
                        break;

                    default:
                        await ProcessBnplInstallmentPaymentAsync(order, invoice, paymentRequest);
                        break;
                }

                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Full payment done for OrderId={OrderId}, PaymentAmount={PaymentAmount}", order.OrderID, paymentRequest.PaymentAmount);
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to process payment for InvoiceId={InvoiceId}", paymentRequest.InvoiceId);
                throw;
            }
        }

        //Helper : ProcessFullPaymentAsync
        private async Task ProcessFullPaymentAsync(CustomerOrder order, Invoice invoice, PaymentRequestDto paymentRequest)
        {
            ValidateBeforePayment(order);

            if (paymentRequest.PaymentAmount != order.TotalAmount)
                throw new Exception("Full payment must match order total");

            order.OrderStatus = OrderStatusEnum.Processing;
            order.OrderPaymentStatus = OrderPaymentStatusEnum.Fully_Paid;
            invoice.InvoiceStatus = InvoiceStatusEnum.Paid;
            invoice.PaidAt = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            var cashflow = await _cashflowService.BuildCashflowAddRequestAsync(
                paymentRequest,
                CashflowTypeEnum.FullPayment
            );

            invoice.Cashflow = cashflow;
        }

        //Helper : ProcessInitialBnplPaymentAsync
        private async Task ProcessInitialBnplPaymentAsync(CustomerOrder order, Invoice invoice, PaymentRequestDto paymentRequest)
        {
            ValidateBeforePayment(order);

            var cashflow = await _cashflowService.BuildCashflowAddRequestAsync(
                paymentRequest,
                CashflowTypeEnum.BnplInitialPayment
            );

            invoice.Cashflow = cashflow;
            invoice.InvoiceStatus = InvoiceStatusEnum.Paid;
            invoice.PaidAt = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            order.BNPL_PLAN!.Bnpl_Status = BnplStatusEnum.Active;
            order.OrderStatus = OrderStatusEnum.Processing;
            order.OrderPaymentStatus = OrderPaymentStatusEnum.Partially_Paid;
        }

        //Helper : ProcessBnplInstallmentPaymentAsync
        private async Task ProcessBnplInstallmentPaymentAsync(CustomerOrder order,Invoice invoice, PaymentRequestDto paymentRequest)
        {
            ValidateBeforePayment(order);

            if (string.IsNullOrWhiteSpace(invoice.SettlementSnapshotJson) || string.IsNullOrWhiteSpace(invoice.SettlementSnapshotHash))
            {
                throw new InvalidOperationException(
                    "Missing settlement snapshot or hash on invoice");
            }

            // 1. Deserialize frozen snapshot
            var frozenSnapshot = JsonSerializer.Deserialize<BnplLatestSnapshotSettledResultDto>(invoice.SettlementSnapshotJson)
                ?? throw new Exception("Settlement snapshot missing");

            // 2. Verify hash
            var canonicalJson = SnapshotHashHelper.SerializeCanonical(frozenSnapshot);

            var computedHash = SnapshotHashHelper.BuildHash(canonicalJson);

            if (!string.Equals(computedHash, invoice.SettlementSnapshotHash, StringComparison.Ordinal))
            {
                throw new SecurityException("Settlement snapshot integrity violation detected");
            }

            // 3. APPLY SNAPSHOT
            _bnpl_planSettlementSummaryService.ApplyFrozenSettlementSnapshot(order, frozenSnapshot);

            // 4. Update installments
            _bNPL_InstallmentService.BuildBnplInstallmentSettlement(order, frozenSnapshot);

            // 5. Build cashflow
            var cashflow = await _cashflowService.BuildCashflowAddRequestAsync(paymentRequest, CashflowTypeEnum.BnplInstallmentPayment);

            invoice.Cashflow = cashflow;
            invoice.InvoiceStatus = InvoiceStatusEnum.Paid;
            invoice.PaidAt = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);
        }

        //Helper : to valiadate payment
        private void ValidateBeforePayment(CustomerOrder order)
        {
            switch (order.OrderStatus)
            {
                // These states allow payments
                case OrderStatusEnum.Pending:
                case OrderStatusEnum.Shipped:
                case OrderStatusEnum.Delivered:
                case OrderStatusEnum.DeliveredAfterCancellationRejected:
                    break;

                case OrderStatusEnum.Cancel_Pending:
                    throw new InvalidOperationException("Payment cannot be processed while a cancellation request is pending.");

                case OrderStatusEnum.Cancelled:
                    throw new InvalidOperationException("Payment cannot be processed for cancelled orders.");

                default:
                    throw new InvalidOperationException("Invalid order status.");
            }

            // 2. BNPL Plan Validation
            var bnplPlan = order.BNPL_PLAN;
            if (bnplPlan == null)
            {
                //full_payment
                return;
            }
            else
            {
                //Bnpl_payment
                if (bnplPlan.Bnpl_Status == BnplStatusEnum.Completed)
                    throw new InvalidOperationException("BNPL plan is already completed. No additional payments are allowed.");

                if (bnplPlan.Bnpl_RemainingInstallmentCount <= 0)
                    throw new InvalidOperationException("No remaining installments exist for this BNPL plan.");
            }
        }
    }
}