using WebApplication1.DTOs.RequestDto.Custom;
using WebApplication1.DTOs.RequestDto.Payment;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.Utils.Helpers;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.ServiceImpl
{
    public class PaymentServiceImpl : IPaymentService
    {
        private readonly ICashflowRepository _cashflowRepository;
        private readonly IBNPL_PlanRepository _bNPL_PlanRepository;
        private readonly ICustomerOrderService _customerOrderService;
        private readonly IBNPL_InstallmentService _bNPL_InstallmentService;
        private readonly IBNPL_PlanSettlementSummaryService _bnpl_planSettlementSummaryService;

        //logger: for auditing
        private readonly ILogger<PaymentServiceImpl> _logger;

        // Constructor
        public PaymentServiceImpl(ICashflowRepository cashflowRepository, IBNPL_PlanRepository bNPL_PlanRepository, ICustomerOrderService customerOrderService, IBNPL_InstallmentService bNPL_InstallmentService, IBNPL_PlanSettlementSummaryService bnpl_planSettlementSummaryService, ILogger<PaymentServiceImpl> logger)
        {
            // Dependency injection
            _cashflowRepository = cashflowRepository;
            _bNPL_PlanRepository = bNPL_PlanRepository;
            _customerOrderService = customerOrderService;
            _bNPL_InstallmentService = bNPL_InstallmentService;
            _bnpl_planSettlementSummaryService = bnpl_planSettlementSummaryService;
            _logger = logger;
        }

        public async Task ProcessFullPaymentPaymentAsync(PaymentRequestDto paymentRequest)
        {
            if (paymentRequest == null)
                throw new ArgumentNullException(nameof(paymentRequest));

            await using var transaction = await _cashflowRepository.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("Starting full payment processing for OrderID={OrderId}, Amount={Amount}",
                    paymentRequest.OrderId, paymentRequest.PaymentAmount);

                // 1. Create a cashflow record
                var cashflow = await GenerateCashFlow(paymentRequest, CashflowTypeEnum.FullPayment);
                _logger.LogInformation("Generated Cashflow record: {CashflowRef}", cashflow.CashflowRef);

                // 2. Update customer order payment status to Fully Paid
                var customerOrderStatusChangeRequest = new CustomerOrderUpdateDto
                {
                    PaymentStatus = OrderPaymentStatusEnum.Fully_Paid
                };

                var customerOrder = await _customerOrderService.UpdateCustomerOrderAsync(paymentRequest.OrderId, customerOrderStatusChangeRequest);
                _logger.LogInformation("Updated customer order payment status to Fully Paid for OrderID={OrderId}", paymentRequest.OrderId);

                // 3. Commit the transaction
                await transaction.CommitAsync();
                _logger.LogInformation("Full payment processed successfully for OrderID={OrderId}", paymentRequest.OrderId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to process full payment for OrderID={OrderId}", paymentRequest.OrderId);
                throw;
            }
        }


        // Initial payment ???

        //Helper method : create cashflow record
        private async Task<Cashflow> GenerateCashFlow(PaymentRequestDto paymentRequest, CashflowTypeEnum cashflowType)
        {
            return await Task.Run(() =>
            {
                if (paymentRequest == null)
                    throw new ArgumentNullException(nameof(paymentRequest));

                // Determine status (default: Paid)
                var status = CashflowStatusEnum.Paid;

                var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);
                // Build reference
                var cashflowRef =
                    $"CF-{status}-{cashflowType}-{now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..6]}";

                var newCashflow = new Cashflow
                {
                    OrderID = paymentRequest.OrderId,
                    AmountPaid = paymentRequest.PaymentAmount,
                    CashflowDate = now,
                    CashflowStatus = status,
                    CashflowRef = cashflowRef
                };

                return newCashflow;
            });
        }

        public async Task ProcessBnplInstallmentPaymentAsync(PaymentRequestDto paymentRequest)
        {
            await using var transaction = await _cashflowRepository.BeginTransactionAsync();
            try
            {
                // 1. Apply BNPL installment payment
                var paymentResult = await _bNPL_InstallmentService.ApplyBnplInstallmentPaymentAsync(paymentRequest);
                _logger.LogInformation("Applied installment payment: {PaymentResult}", paymentResult);

                // 2. Retrieve associated BNPL plan
                var plan = await _bNPL_PlanRepository.GetByOrderIdAsync(paymentRequest.OrderId);
                if (plan == null)
                    throw new Exception($"Associated BNPL plan not found for OrderID={paymentRequest.OrderId}");

                // 3. Generate settlement snapshot
                var settlementSnapshot = await _bnpl_planSettlementSummaryService.GenerateSettlementAsync(plan.Bnpl_PlanID);
                _logger.LogInformation("Generated settlement snapshot for PlanID={PlanId}", plan.Bnpl_PlanID);

                // 4. Generate cashflow record
                var cashflow = await GenerateCashFlow(paymentRequest, CashflowTypeEnum.BnplInstallmentPayment);
                _logger.LogInformation("Generated Cashflow record: {CashflowRef}", cashflow.CashflowRef);

                // 5. Commit transaction
                await transaction.CommitAsync();
                _logger.LogInformation("BNPL installment payment processed successfully for OrderID={OrderId}", paymentRequest.OrderId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to process BNPL installment payment for OrderID={OrderId}", paymentRequest.OrderId);
                throw;
            }
        }
    }
}