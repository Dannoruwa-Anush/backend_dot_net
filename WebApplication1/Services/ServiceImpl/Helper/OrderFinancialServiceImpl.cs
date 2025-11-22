using WebApplication1.Models;
using WebApplication1.Services.IService;
using WebApplication1.Services.IService.Helper;
using WebApplication1.Utils.Helpers;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.ServiceImpl.Helper
{
    public class OrderFinancialServiceImpl : IOrderFinancialService
    {
        private readonly IBNPL_PlanService _bNPL_PlanService;
        private readonly IBNPL_InstallmentService _bNPL_InstallmentService;
        private readonly IBNPL_PlanSettlementSummaryService _bnplSettlementService;
        private readonly ICashflowService _cashflowService;
        private readonly ILogger<OrderFinancialServiceImpl> _logger;

        public OrderFinancialServiceImpl(
            IBNPL_PlanService bNPL_PlanService,
            IBNPL_InstallmentService bNPL_InstallmentService,
            IBNPL_PlanSettlementSummaryService bnplSettlementService,
            ICashflowService cashflowService,
            ILogger<OrderFinancialServiceImpl> logger)
        {
            _bNPL_PlanService = bNPL_PlanService;
            _bNPL_InstallmentService = bNPL_InstallmentService;
            _bnplSettlementService = bnplSettlementService;
            _cashflowService = cashflowService;
            _logger = logger;
        }

        public async Task BuildPaymentUpdateRequestAsync(CustomerOrder order, OrderPaymentStatusEnum newStatus)
        {
            var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);
            
            if (order.OrderPaymentStatus == newStatus)
                return;

            ValidatePaymentStatusTransition(order.OrderPaymentStatus, newStatus);

            if(order.OrderPaymentStatus == OrderPaymentStatusEnum.Refunded)
            {
                await BuildPaymentRefundUpdateRequestAsync(order, now);
            }

            var totalPaid = await _cashflowService.SumCashflowsByOrderAsync(order.OrderID);
            order.OrderPaymentStatus = totalPaid >= order.TotalAmount
                ? OrderPaymentStatusEnum.Fully_Paid
                : newStatus;

            if (order.OrderPaymentStatus == OrderPaymentStatusEnum.Fully_Paid)
                order.PaymentCompletedDate = now;

            await BuildPaymentOngoingUpdateRequestAsync(order, now);     
        }

        //Helper method : ValidatePaymentStatusTransition
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

        //Helper Method : ongoing/ completed
        private async Task BuildPaymentRefundUpdateRequestAsync(CustomerOrder order, DateTime now)
        {
            // Cashflow status
            await _cashflowService.BuildCashflowStatusUpdateRequestAsync(
                order, CashflowStatusEnum.Refunded, now);

            if (order.BNPL_PLAN != null)
            {
                // BNPL plan
                await _bNPL_PlanService.BuildBnplPlanStatusUpdateRequestAsync(
                    order.BNPL_PLAN, BnplStatusEnum.Cancelled, now);

                // Installments
                await _bNPL_InstallmentService.BuildBnplInstallmetStatusUpdateRequestAsync(
                    order.BNPL_PLAN.BNPL_Installments, BNPL_Installment_StatusEnum.Refunded, now);

                // Settlement snapshot
                await _bnplSettlementService.BuildBnplSettlementSummaryStatusUpdateRequestAsync(
                    order.BNPL_PLAN.BNPL_PlanSettlementSummaries,
                    BNPL_PlanSettlementSummary_StatusEnum.Cancelled,
                    now);
            }

            _logger.LogInformation("Order refund processing complete for OrderID={OrderId}", order.OrderID);
        }

        //Helper Method : terminate/refund
        private async Task BuildPaymentOngoingUpdateRequestAsync(CustomerOrder order, DateTime now)
        {
            if (order.BNPL_PLAN != null)
            {
                // BNPL plan
                await _bNPL_PlanService.BuildBnplOngoingPlanStatusUpdateRequestAsync(
                    order.BNPL_PLAN, now);
            }

            _logger.LogInformation("Order refund processing complete for OrderID={OrderId}", order.OrderID);
        }
    }
}