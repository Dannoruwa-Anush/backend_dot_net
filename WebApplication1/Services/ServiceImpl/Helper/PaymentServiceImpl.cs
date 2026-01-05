using WebApplication1.DTOs.RequestDto.Payment;
using WebApplication1.Models;
using WebApplication1.Services.IService;
using WebApplication1.Services.IService.Helper;
using WebApplication1.UOW.IUOW;
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
            if (paymentRequest.PaymentAmount <= 0)
                throw new Exception("Payment amount should be a positive number");
            //------------- [ start - todo can simplify] ---------
            var invoice = await _invoiceService.GetInvoiceByIdAsync(paymentRequest.InvoiceId)
                              ?? throw new Exception("Invoice not found");

            var existingOrder = await _customerOrderService
                    .GetCustomerOrderWithFinancialDetailsByIdAsync(invoice.OrderID);

            if (existingOrder == null)
                throw new Exception("Order not found");
            //------------- [ End - todo can simplify] ---------

            ValidateBeforePayment(existingOrder);

            if (paymentRequest.PaymentAmount != existingOrder.TotalAmount)
                throw new Exception("Full payment must match the total order amount");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                switch (invoice.InvoiceType)
                {
                    case InvoiceTypeEnum.Full_Payment_Invoice:
                        await ProcessFullPaymentAsync(existingOrder, invoice, paymentRequest);
                        break;

                    case InvoiceTypeEnum.Bnpl_Initial_Payment_Invoice:
                        await ProcessInitialBnplPaymentAsync(existingOrder, invoice, paymentRequest);
                        break;

                    default:
                        await ProcessBnplInstallmentPaymentAsync(existingOrder, invoice, paymentRequest);
                        break;
                }

                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Full payment done for OrderId={OrderId}, PaymentAmount={PaymentAmount}", existingOrder.OrderID, paymentRequest.PaymentAmount);
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

            order.OrderPaymentStatus = OrderPaymentStatusEnum.Fully_Paid;
            invoice.InvoiceStatus = InvoiceStatusEnum.Paid;

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

            order.BNPL_PLAN!.Bnpl_Status = BnplStatusEnum.Active;
            order.OrderPaymentStatus = OrderPaymentStatusEnum.Partially_Paid;
        }

        //Helper : ProcessBnplInstallmentPaymentAsync
        private async Task ProcessBnplInstallmentPaymentAsync(CustomerOrder order, Invoice invoice, PaymentRequestDto paymentRequest)
        {
            ValidateBeforePayment(order);

            // Apply payment to snapshot
            var latestSnapshotSettledResult = _bnpl_planSettlementSummaryService.BuildBNPL_PlanLatestSettlementSummaryUpdateRequest(order, paymentRequest.PaymentAmount);

            // Update the installments according to the payment
            var paymentResult = _bNPL_InstallmentService.BuildBnplInstallmentSettlement(order, latestSnapshotSettledResult);

            // Build cashflow
            var cashflow = await _cashflowService.BuildCashflowAddRequestAsync(paymentRequest, CashflowTypeEnum.BnplInstallmentPayment);
            invoice.Cashflow = cashflow;
            invoice.InvoiceStatus = InvoiceStatusEnum.Paid;
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