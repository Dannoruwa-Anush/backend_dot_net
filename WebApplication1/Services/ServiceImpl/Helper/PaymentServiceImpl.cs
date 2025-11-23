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

        public async Task<BnplInstallmentPaymentResultDto?> ProcessPaymentAsync(PaymentRequestDto paymentRequest, BNPLInstallmentCalculatorRequestDto? initialBnplRequest = null)
        {
            if (paymentRequest == null)
                throw new ArgumentNullException(nameof(paymentRequest));

            var order = await _customerOrderService.GetCustomerOrderByIdAsync(paymentRequest.OrderId);
            if (order == null)
                throw new InvalidOperationException("Customer order not found.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                BnplInstallmentPaymentResultDto? paymentResult = null;

                if (order.OrderPaymentStatus == OrderPaymentStatusEnum.Pending)
                {
                    // Full Payment
                    await HandleFullPaymentAsync(order, paymentRequest);
                }
                else if (order.BNPL_PLAN == null)
                {
                    // BNPL Initial Payment
                    if (initialBnplRequest == null)
                        throw new InvalidOperationException("Initial BNPL request required for first installment.");

                    await HandleBnplInitialPaymentAsync(order, paymentRequest, initialBnplRequest);
                }
                else
                {
                    // BNPL Installment Payment
                    paymentResult = await HandleBnplInstallmentPaymentAsync(order, paymentRequest);
                }

                await _unitOfWork.CommitAsync();
                return paymentResult;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to process payment for OrderID={OrderId}", paymentRequest.OrderId);
                throw;
            }
        }

        //Helper : FullPayment
        private async Task HandleFullPaymentAsync(CustomerOrder order, PaymentRequestDto paymentRequest)
        {
            ValidatePaymentStatusTransition(order.OrderPaymentStatus, OrderPaymentStatusEnum.Fully_Paid);

            // Create cashflow
            await _cashflowService.BuildCashflowAddRequestAsync(paymentRequest, CashflowTypeEnum.FullPayment);

            // Update order
            order.OrderPaymentStatus = OrderPaymentStatusEnum.Fully_Paid;

            _logger.LogInformation("Full payment processed for OrderID={OrderId}", order.OrderID);
        }

        //Helper : Bnpl Initial Payment (Bnpl_plan will be created after the initial payment)
        private async Task HandleBnplInitialPaymentAsync(CustomerOrder order, PaymentRequestDto paymentRequest, BNPLInstallmentCalculatorRequestDto initialBnplRequest)
        {
            var bnplCalc = await _bNPL_PlanService.CalculateBNPL_PlanAmountPerInstallmentAsync(initialBnplRequest);

            // Create BNPL plan
            var bnplPlan = await _bNPL_PlanService.BuildBnpl_PlanAddRequestAsync(new BNPL_PLAN
            {
                Bnpl_InitialPayment = initialBnplRequest.InitialPayment,
                Bnpl_AmountPerInstallment = bnplCalc.AmountPerInstallment,
                Bnpl_TotalInstallmentCount = initialBnplRequest.InstallmentCount,
                Bnpl_PlanTypeID = initialBnplRequest.Bnpl_PlanTypeID,
                OrderID = initialBnplRequest.OrderID,
            });

            // Create installments
            await _bNPL_InstallmentService.BuildBnplInstallmentBulkAddRequestAsync(bnplPlan);

            // Cashflow for initial payment
            await _cashflowService.BuildCashflowAddRequestAsync(paymentRequest, CashflowTypeEnum.BnplInitialPayment);

            // Generate settlement snapshot
            await _bnpl_planSettlementSummaryService.BuildSettlementGenerateRequestAsync(bnplPlan.Bnpl_PlanID);

            // Update order
            order.OrderPaymentStatus = OrderPaymentStatusEnum.Partially_Paid;

            _logger.LogInformation("BNPL initial payment processed for OrderID={OrderId}", order.OrderID);
        }

        //Hepler
        private async Task<BnplInstallmentPaymentResultDto> HandleBnplInstallmentPaymentAsync(CustomerOrder order, PaymentRequestDto paymentRequest)
        {
            // Apply installment payment
            var (paymentResult, updatedInstallments) = await _bNPL_InstallmentService.BuildBnplInstallmentSettlementAsync(paymentRequest);

            // Create cashflow
            await _cashflowService.BuildCashflowAddRequestAsync(paymentRequest, CashflowTypeEnum.BnplInstallmentPayment);

            // Update BNPL plan & order status
            await UpdateBnplPlanAfterPaymentAsync(order);

            // Generate settlement snapshot
            await _bnpl_planSettlementSummaryService.BuildSettlementGenerateRequestAsync(order.BNPL_PLAN!.Bnpl_PlanID);

            _logger.LogInformation("BNPL installment payment processed for OrderID={OrderId}", order.OrderID);

            return paymentResult;
        }

        //Helper : Update Bnpl plan 
        private async Task UpdateBnplPlanAfterPaymentAsync(CustomerOrder order)
        {
            var plan = order.BNPL_PLAN!;
            var installments = await _bNPL_InstallmentService.GetAllUnsettledInstallmentByPlanIdAsync(plan.Bnpl_PlanID);

            var remaining = installments.Count(i => i.TotalPaid < i.TotalDueAmount);

            if (remaining == 0)
            {
                plan.Bnpl_RemainingInstallmentCount = 0;
                plan.Bnpl_NextDueDate = null;

                foreach (var inst in installments)
                {
                    if (inst.Bnpl_Installment_Status != BNPL_Installment_StatusEnum.Paid_OnTime &&
                        inst.Bnpl_Installment_Status != BNPL_Installment_StatusEnum.Paid_Late)
                    {
                        inst.Bnpl_Installment_Status = inst.Installment_DueDate < TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow)
                            ? BNPL_Installment_StatusEnum.Paid_Late
                            : BNPL_Installment_StatusEnum.Paid_OnTime;
                    }
                }

                ValidatePaymentStatusTransition(order.OrderPaymentStatus, OrderPaymentStatusEnum.Fully_Paid);
                order.OrderPaymentStatus = OrderPaymentStatusEnum.Fully_Paid;
            }
            else
            {
                plan.Bnpl_RemainingInstallmentCount = remaining;

                var nextInstallment = installments
                    .Where(i => i.TotalPaid < i.TotalDueAmount)
                    .OrderBy(i => i.Installment_DueDate)
                    .FirstOrDefault();

                plan.Bnpl_NextDueDate = nextInstallment?.Installment_DueDate;

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