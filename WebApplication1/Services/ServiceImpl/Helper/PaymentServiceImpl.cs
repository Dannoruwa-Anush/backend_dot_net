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
            var invoice = await _invoiceService.GetInvoiceWithOrderAsync(paymentRequest.InvoiceId)
                ?? throw new Exception("Invoice not found");

            if (invoice.InvoiceStatus == InvoiceStatusEnum.Paid)
                throw new InvalidOperationException("Invoice already paid");

            var order = invoice.CustomerOrder
                ?? throw new Exception("Order not loaded");

            ValidateBeforePayment(order);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                switch (invoice.InvoiceType)
                {
                    case InvoiceTypeEnum.Full_Pay:
                        await ProcessFullPaymentAsync(order, invoice, paymentRequest);
                        break;

                    case InvoiceTypeEnum.Bnpl_Initial_Pay:
                        await ProcessInitialBnplPaymentAsync(order, invoice, paymentRequest);
                        break;

                    case InvoiceTypeEnum.Bnpl_Installment_Pay:
                        await ProcessBnplInstallmentPaymentAsync(order, invoice, paymentRequest);
                        break;

                    default:
                        throw new InvalidOperationException("Unsupported invoice type");
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            // Side-effect AFTER payment commit
            await _invoiceService.GenerateReceiptAsync(invoice.InvoiceID);

            return invoice;
        }

        //Helper : ProcessFullPaymentAsync
        private async Task ProcessFullPaymentAsync(CustomerOrder order, Invoice invoice, PaymentRequestDto paymentRequest)
        {
            order.OrderStatus = OrderStatusEnum.Processing;
            order.OrderPaymentStatus = OrderPaymentStatusEnum.Fully_Paid;

            invoice.InvoiceStatus = InvoiceStatusEnum.Paid;
            invoice.PaidAt = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            invoice.Cashflow = await _cashflowService
                .BuildCashflowAddRequestAsync(paymentRequest, CashflowTypeEnum.FullPayment);
        }

        //Helper : ProcessInitialBnplPaymentAsync
        private async Task ProcessInitialBnplPaymentAsync(CustomerOrder order, Invoice invoice, PaymentRequestDto paymentRequest)
        {
            invoice.InvoiceStatus = InvoiceStatusEnum.Paid;
            invoice.PaidAt = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            invoice.Cashflow = await _cashflowService
                .BuildCashflowAddRequestAsync(paymentRequest, CashflowTypeEnum.BnplInitialPayment);

            order.BNPL_PLAN!.Bnpl_Status = BnplStatusEnum.Active;
            order.OrderStatus = OrderStatusEnum.Processing;
            order.OrderPaymentStatus = OrderPaymentStatusEnum.Partially_Paid;
        }

        //Helper : ProcessBnplInstallmentPaymentAsync
        private async Task ProcessBnplInstallmentPaymentAsync(CustomerOrder order, Invoice invoice, PaymentRequestDto paymentRequest)
        {
            if (string.IsNullOrWhiteSpace(invoice.SettlementSnapshotJson) ||
                string.IsNullOrWhiteSpace(invoice.SettlementSnapshotHash))
                throw new SecurityException("Missing settlement snapshot or hash");

            var frozenSnapshot = JsonSerializer.Deserialize<BnplLatestSnapshotSettledResultDto>(
                invoice.SettlementSnapshotJson)
                ?? throw new Exception("Invalid settlement snapshot");

            var canonicalJson = SnapshotHashHelper.SerializeCanonical(frozenSnapshot);
            var computedHash = SnapshotHashHelper.BuildHash(canonicalJson);

            if (!string.Equals(computedHash, invoice.SettlementSnapshotHash, StringComparison.Ordinal))
                throw new SecurityException("Settlement snapshot integrity violation");

            _bnpl_planSettlementSummaryService
                .ApplyFrozenSettlementSnapshot(order, frozenSnapshot);

            _bNPL_InstallmentService
                .BuildBnplInstallmentSettlement(order, frozenSnapshot);

            invoice.Cashflow = await _cashflowService
                .BuildCashflowAddRequestAsync(paymentRequest, CashflowTypeEnum.BnplInstallmentPayment);

            invoice.InvoiceStatus = InvoiceStatusEnum.Paid;
            invoice.PaidAt = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            if (order.BNPL_PLAN!.Bnpl_RemainingInstallmentCount == 0)
            {
                order.BNPL_PLAN.Bnpl_Status = BnplStatusEnum.Completed;
                order.OrderPaymentStatus = OrderPaymentStatusEnum.Fully_Paid;
            }
            else
            {
                order.OrderPaymentStatus = OrderPaymentStatusEnum.Partially_Paid;
            }
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