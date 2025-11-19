using WebApplication1.DTOs.RequestDto.BnplCal;
using WebApplication1.DTOs.RequestDto.Custom;
using WebApplication1.DTOs.RequestDto.Payment;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.Utils.Helpers;
using WebApplication1.Utils.Project_Enums;
using WebApplication1.Utils.SystemConstants;

namespace WebApplication1.Services.ServiceImpl
{
    public class PaymentServiceImpl : IPaymentService
    {
        private readonly ICashflowRepository _cashflowRepository;
        private readonly IBNPL_PlanRepository _bNPL_PlanRepository;

        //****
        private readonly ICashflowService _cashflowService;
        private readonly ICustomerOrderService _customerOrderService;
        private readonly IBNPL_InstallmentService _bNPL_InstallmentService;
        private readonly IBNPL_PlanSettlementSummaryService _bnpl_planSettlementSummaryService;
        private readonly IBNPL_PlanService _bNPL_PlanService;

        //logger: for auditing
        private readonly ILogger<PaymentServiceImpl> _logger;

        // Constructor
        public PaymentServiceImpl(ICashflowRepository cashflowRepository, IBNPL_PlanRepository bNPL_PlanRepository, ICashflowService cashflowService, ICustomerOrderService customerOrderService, IBNPL_InstallmentService bNPL_InstallmentService, IBNPL_PlanSettlementSummaryService bnpl_planSettlementSummaryService, IBNPL_PlanService bNPL_PlanService, ILogger<PaymentServiceImpl> logger)
        {
            // Dependency injection
            _cashflowRepository = cashflowRepository;
            _bNPL_PlanRepository = bNPL_PlanRepository;
            _cashflowService = cashflowService;
            _customerOrderService = customerOrderService;
            _bNPL_InstallmentService = bNPL_InstallmentService;
            _bnpl_planSettlementSummaryService = bnpl_planSettlementSummaryService;
            _bNPL_PlanService = bNPL_PlanService;
            _logger = logger;
        }

        // Full Payment
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
                var cashflow = await _cashflowService.AddCashflowAsync(paymentRequest, CashflowTypeEnum.FullPayment);
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

        // BNPL : Initial Payment
        public async Task ProcessBnplInitialPaymentAsync(BNPLInstallmentCalculatorRequestDto request)
        {
            await using var transaction = await _cashflowRepository.BeginTransactionAsync();
            try
            {
                var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

                // Calculate BNPL values
                var bnplCalc = await _bNPL_PlanService.CalculateBNPL_PlanAmountPerInstallmentAsync(request);

                // Create the BNPL plan (no transaction inside)
                var bnpl_plan = await _bNPL_PlanService.AddBNPL_PlanAsync(new BNPL_PLAN
                {
                    OrderID = request.OrderID,
                    Bnpl_PlanTypeID = request.Bnpl_PlanTypeID,
                    Bnpl_TotalInstallmentCount = request.InstallmentCount,
                    Bnpl_AmountPerInstallment = bnplCalc.AmountPerInstallment
                });

                // Generate installments
                await _bNPL_InstallmentService.AddBnplInstallmentsAsync(bnpl_plan);

                // Create a cashflow for initial payment
                var paymentRequest = new PaymentRequestDto
                {
                    PaymentAmount = request.InitialPayment,
                    OrderId = request.OrderID
                };

                var cashflow = await _cashflowService.AddCashflowAsync(
                    paymentRequest,
                    CashflowTypeEnum.BnplInitialPayment
                );

                // Generate settlement snapshot
                await _bnpl_planSettlementSummaryService.GenerateSettlementAsync(bnpl_plan.Bnpl_PlanID);

                // Commit
                await transaction.CommitAsync();

                _logger.LogInformation("BNPL initial payment processed successfully for OrderID={OrderId}", request.OrderID);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to process BNPL initial payment for OrderID={OrderId}", request.OrderID);
                throw;
            }
        }

        // BNPL : Installment Payment
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
                var cashflow = await _cashflowService.AddCashflowAsync(paymentRequest, CashflowTypeEnum.BnplInstallmentPayment);
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