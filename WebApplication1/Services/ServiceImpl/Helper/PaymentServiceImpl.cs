using WebApplication1.DTOs.RequestDto.BnplCal;
using WebApplication1.DTOs.RequestDto.Payment;
using WebApplication1.DTOs.ResponseDto.Payment.Bnpl;
using WebApplication1.Models;
using WebApplication1.Services.IService;
using WebApplication1.Services.IService.Helper;
using WebApplication1.UOW.IUOW;
using WebApplication1.Utils.Helpers;
using WebApplication1.Utils.Project_Enums;
using WebApplication1.Utils.SystemConstants;

namespace WebApplication1.Services.ServiceImpl.Helper
{
    public class PaymentServiceImpl : IPaymentService
    {
        private readonly IAppUnitOfWork _unitOfWork;

        private readonly ICustomerOrderService _customerOrderService;
        private readonly ICashflowService _cashflowService;
        private readonly IBNPL_InstallmentService _bNPL_InstallmentService;
        private readonly IBNPL_PlanSettlementSummaryService _bnpl_planSettlementSummaryService;
        private readonly IBNPL_PlanService _bNPL_PlanService;

        //logger: for auditing
        private readonly ILogger<PaymentServiceImpl> _logger;

        // Constructor
        public PaymentServiceImpl(
        IAppUnitOfWork unitOfWork,

        ICustomerOrderService customerOrderService,
        ICashflowService cashflowService,
        IBNPL_InstallmentService bNPL_InstallmentService,
        IBNPL_PlanSettlementSummaryService bnpl_planSettlementSummaryService,
        IBNPL_PlanService bNPL_PlanService,
        ILogger<PaymentServiceImpl> logger)
        {
            // Dependency injection
            _unitOfWork = unitOfWork;

            _customerOrderService = customerOrderService;
            _cashflowService = cashflowService;
            _bNPL_InstallmentService = bNPL_InstallmentService;
            _bnpl_planSettlementSummaryService = bnpl_planSettlementSummaryService;
            _bNPL_PlanService = bNPL_PlanService;
            _logger = logger;
        }

        //Full payment
        public async Task<bool> ProcessFullPaymentAsync(PaymentRequestDto paymentRequest)
        {
            if (paymentRequest.PaymentAmount <= 0)
                throw new Exception("Payment amount should be a positive number");

            var existingOrder = await _customerOrderService.GetCustomerOrderByIdAsync(paymentRequest.OrderId);
            if (existingOrder == null)
                throw new Exception("Order not found");

            ValidateBeforePayment(existingOrder);

            if (paymentRequest.PaymentAmount != existingOrder.TotalAmount)
                throw new Exception("Full payment must match the total order amount");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Create a cashflow
                var cashflow = await _cashflowService.BuildCashflowAddRequestAsync(paymentRequest, CashflowTypeEnum.FullPayment);
                //existingOrder.Cashflows.Add(cashflow);

                // Update order payment status
                existingOrder.OrderPaymentStatus = OrderPaymentStatusEnum.Fully_Paid;

                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Full payment done for OrderId={OrderId}, PaymentAmount={PaymentAmount}", existingOrder.OrderID, paymentRequest.PaymentAmount);
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to process full payment for OrderID={OrderID}", paymentRequest.OrderId);
                throw;
            }
        }

        //Bnpl initial payment
        public async Task<bool> ProcessInitialBnplPaymentAsync(BnplInitialPaymentRequestDto request)
        {
            //if (request.InitialPayment <= 0)
                //throw new Exception("Initial payment amount should be a positive number");

            var existingOrder = await _customerOrderService.GetCustomerOrderByIdAsync(request.OrderId);
            if (existingOrder == null)
                throw new Exception("Order not found");

            ValidateBeforePayment(existingOrder);    

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                /*
                var bnplCalc = await _bNPL_PlanService.CalculateBNPL_PlanAmountPerInstallmentAsync(
                    new BNPLInstallmentCalculatorRequestDto
                    {
                        OrderID = request.OrderId,
                        InitialPayment = request.InitialPayment,
                        Bnpl_PlanTypeID = request.Bnpl_PlanTypeID,
                        InstallmentCount = request.InstallmentCount
                    });

                // Create new bnpl_plan object
                var newBnplPlan = await _bNPL_PlanService.BuildBnpl_PlanAddRequestAsync(
                    new BNPL_PLAN
                    {
                        Bnpl_InitialPayment = request.InitialPayment,
                        Bnpl_AmountPerInstallment = bnplCalc.AmountPerInstallment,
                        Bnpl_TotalInstallmentCount = request.InstallmentCount,
                        Bnpl_PlanTypeID = request.Bnpl_PlanTypeID,
                        OrderID = request.OrderId,
                    });

                // Build installments
                var installments = await _bNPL_InstallmentService.BuildBnplInstallmentBulkAddRequestAsync(newBnplPlan);
                foreach (var inst in installments)
                    newBnplPlan.BNPL_Installments.Add(inst);

                // Build initial snapshot
                var snapshot = _bnpl_planSettlementSummaryService.BuildSettlementGenerateRequestForPlan(newBnplPlan);
                if (snapshot != null)
                    newBnplPlan.BNPL_PlanSettlementSummaries.Add(snapshot);

                */
                
                // Build cashflow
                var cashflow = await _cashflowService.BuildCashflowAddRequestAsync(
                    new PaymentRequestDto
                    {
                        OrderId = request.OrderId,
                        PaymentAmount = request.InitialPayment
                    },
                    CashflowTypeEnum.BnplInitialPayment
                );

                existingOrder.BNPL_PLAN!.Bnpl_Status = BnplStatusEnum.Active;
                //existingOrder.Cashflows.Add(cashflow);
                existingOrder.OrderPaymentStatus = OrderPaymentStatusEnum.Partially_Paid;

                // Save all together
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Bnpl initial payment done for OrderId={OrderId}, PaymentAmount={PaymentAmount}", existingOrder.OrderID, request.InitialPayment);
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to process bnpl initial payment for OrderID={OrderID}", request.OrderId);
                throw;
            }
        }

        //Bnpl installment payment
        public async Task<bool> ProcessBnplInstallmentPaymentAsync(PaymentRequestDto paymentRequest)
        {
            /*
            if (paymentRequest.PaymentAmount <= 0)
                throw new Exception("Payment amount should be a positive number");
            */
            var existingOrder = await _customerOrderService.GetCustomerOrderWithFinancialDetailsByIdAsync(paymentRequest.OrderId);

            if (existingOrder == null)
                throw new Exception("Order not found");

            ValidateBeforePayment(existingOrder);    

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Apply payment to snapshot
                var latestSnapshotSettledResult = _bnpl_planSettlementSummaryService.BuildBNPL_PlanLatestSettlementSummaryUpdateRequest(existingOrder, paymentRequest.PaymentAmount);

                // Update the installments according to the payment
                var paymentResult = _bNPL_InstallmentService.BuildBnplInstallmentSettlement(existingOrder, latestSnapshotSettledResult);

                // Build cashflow
                var cashflow = await _cashflowService.BuildCashflowAddRequestAsync(
                    new PaymentRequestDto
                    {
                        OrderId = paymentRequest.OrderId,
                        PaymentAmount = paymentRequest.PaymentAmount
                    },
                    CashflowTypeEnum.BnplInstallmentPayment
                );
                //existingOrder.Cashflows.Add(cashflow);

                await _unitOfWork.CommitAsync();
                _logger.LogInformation("Installment payment done for OrderId={OrderId}, PaymentAmount={PaymentAmount}", existingOrder.OrderID, paymentRequest.PaymentAmount);
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to process bnpl installment payment for OrderID={OrderID}", paymentRequest.OrderId);
                throw;
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
                    throw new InvalidOperationException( "No remaining installments exist for this BNPL plan.");
            }
        }
    }
}