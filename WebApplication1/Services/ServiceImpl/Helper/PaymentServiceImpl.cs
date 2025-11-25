using WebApplication1.DTOs.RequestDto.BnplCal;
using WebApplication1.DTOs.RequestDto.Payment;
using WebApplication1.DTOs.ResponseDto.Payment.Bnpl;
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
            var order = await _customerOrderService.GetCustomerOrderByIdAsync(paymentRequest.OrderId);
            if (order == null)
                throw new Exception("Order not found");

            if (paymentRequest.PaymentAmount != order.TotalAmount)
                throw new Exception("Full payment must match the total order amount");  

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                ValidatePaymentStatusTransition(order.OrderPaymentStatus, OrderPaymentStatusEnum.Fully_Paid);

                // Create a cashflow
                await _cashflowService.BuildCashflowAddRequestAsync(paymentRequest, CashflowTypeEnum.FullPayment);

                // Update order payment status
                order.OrderPaymentStatus = OrderPaymentStatusEnum.Fully_Paid;

                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Full payment done for OrderId={OrderId}, PaymentAmount={PaymentAmount}", order.OrderID, paymentRequest.PaymentAmount);
                return true;
            }
            catch(Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to process full payment for OrderID={OrderID}", paymentRequest.OrderId);
                throw;
            }
        }

        //Bnpl initial payment
        public async Task<BNPL_PLAN> ProcessInitialBnplPaymentAsync(BnplInitialPaymentRequestDto request)
        {
            var order = await _customerOrderService.GetCustomerOrderByIdAsync(request.OrderId);
            if (order == null)
                throw new Exception("Order not found");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var bnplCalc = await _bNPL_PlanService.CalculateBNPL_PlanAmountPerInstallmentAsync(
                    new BNPLInstallmentCalculatorRequestDto
                    {
                        OrderID = request.OrderId,
                        InitialPayment = request.InitialPayment,
                        Bnpl_PlanTypeID = request.Bnpl_PlanTypeID,
                        InstallmentCount = request.InstallmentCount
                    });

                // Create a BNPL plan
                var bnplPlan = await _bNPL_PlanService.BuildBnpl_PlanAddRequestAsync(
                    new BNPL_PLAN
                    {
                        Bnpl_InitialPayment = request.InitialPayment,
                        Bnpl_AmountPerInstallment = bnplCalc.AmountPerInstallment,
                        Bnpl_TotalInstallmentCount = request.InstallmentCount,
                        Bnpl_PlanTypeID = request.Bnpl_PlanTypeID,
                        OrderID = request.OrderId,
                    });

                // Generate installments for selected bnpl plan
                await _bNPL_InstallmentService.BuildBnplInstallmentBulkAddRequestAsync(bnplPlan);

                // Create the initial cashflow
                await _cashflowService.BuildCashflowAddRequestAsync(
                    new PaymentRequestDto
                    {
                        OrderId = request.OrderId,
                        PaymentAmount = request.InitialPayment
                    },
                    CashflowTypeEnum.BnplInitialPayment);

                // Create the initial snapshot
                await _bnpl_planSettlementSummaryService.BuildSettlementGenerateRequestAsync(bnplPlan.Bnpl_PlanID);

                // Update order payment status
                order.OrderPaymentStatus = OrderPaymentStatusEnum.Partially_Paid;

                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Bnpl initial payment done for OrderId={OrderId}, PaymentAmount={PaymentAmount}", order.OrderID, request.InitialPayment);
                return bnplPlan;
            }
            catch(Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to process bnpl initial payment for OrderID={OrderID}", request.OrderId);
                throw;
            }
        }

        //Bnpl installment payment
        public async Task<BnplInstallmentPaymentResultDto?> ProcessBnplInstallmentPaymentAsync(PaymentRequestDto paymentRequest)
        {
            var order = await _customerOrderService.GetCustomerOrderByIdAsync(paymentRequest.OrderId);
            if (order?.BNPL_PLAN == null)
                throw new Exception("BNPL plan not found");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var (paymentResult, updatedInstallments) =
                    await _bNPL_InstallmentService.BuildBnplInstallmentSettlementAsync(paymentRequest);

                // Create cashflow
                await _cashflowService.BuildCashflowAddRequestAsync(
                    paymentRequest,
                    CashflowTypeEnum.BnplInstallmentPayment);

                // Recalculate order & plan status
                await UpdateBnplPlanStatusAsync(order);

                // Create new snapshot
                await _bnpl_planSettlementSummaryService.BuildSettlementGenerateRequestAsync(order.BNPL_PLAN.Bnpl_PlanID);

                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Installment payment done for OrderId={OrderId}, PaymentAmount={PaymentAmount}", order.OrderID, paymentRequest.PaymentAmount);
                return paymentResult;
            }
            catch(Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to process bnpl installment payment for OrderID={OrderID}", paymentRequest.OrderId);
                throw;
            }
        }

        //Helper : Bnpl plan status upadate
        private async Task UpdateBnplPlanStatusAsync(CustomerOrder order)
        {
            var plan = order.BNPL_PLAN!;
            var installments = await _bNPL_InstallmentService
                .GetAllUnsettledInstallmentByPlanIdAsync(plan.Bnpl_PlanID);

            var remaining = installments.Count(i => i.TotalPaid < i.TotalDueAmount);

            if (remaining == 0)
            {
                plan.Bnpl_RemainingInstallmentCount = 0;
                plan.Bnpl_NextDueDate = null;

                ValidatePaymentStatusTransition(order.OrderPaymentStatus, OrderPaymentStatusEnum.Fully_Paid);
                order.OrderPaymentStatus = OrderPaymentStatusEnum.Fully_Paid;
            }
            else
            {
                plan.Bnpl_RemainingInstallmentCount = remaining;

                plan.Bnpl_NextDueDate = installments
                    .Where(i => i.TotalPaid < i.TotalDueAmount)
                    .OrderBy(i => i.Installment_DueDate)
                    .First()
                    .Installment_DueDate;

                if (order.OrderPaymentStatus != OrderPaymentStatusEnum.Partially_Paid)
                {
                    ValidatePaymentStatusTransition(order.OrderPaymentStatus, OrderPaymentStatusEnum.Partially_Paid);
                    order.OrderPaymentStatus = OrderPaymentStatusEnum.Partially_Paid;
                }
            }
        }
        
        //Helper : ValidatePaymentStatus
        private void ValidatePaymentStatusTransition(OrderPaymentStatusEnum oldStatus, OrderPaymentStatusEnum newStatus)
        {
            switch (oldStatus)
            {
                case OrderPaymentStatusEnum.Partially_Paid:
                    if (newStatus != OrderPaymentStatusEnum.Fully_Paid && newStatus != OrderPaymentStatusEnum.Overdue)
                        throw new InvalidOperationException("Partially paid orders can only move to 'Fully_Paid' or 'Overdue'.");
                    break;
                case OrderPaymentStatusEnum.Fully_Paid:
                    if (newStatus != OrderPaymentStatusEnum.Refunded)
                        throw new InvalidOperationException("Fully paid orders can only move to 'Refunded'.");
                    break;
                case OrderPaymentStatusEnum.Overdue:
                    if (newStatus != OrderPaymentStatusEnum.Partially_Paid && newStatus != OrderPaymentStatusEnum.Fully_Paid)
                        throw new InvalidOperationException("Overdue orders can only move to 'Partially_Paid' or 'Fully_Paid'.");
                    break;
                case OrderPaymentStatusEnum.Refunded:
                    throw new InvalidOperationException("Refunded orders cannot change payment status.");
            }
        }
    }
}