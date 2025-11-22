using WebApplication1.Models;
using WebApplication1.Services.IService;
using WebApplication1.Services.IService.Helper;
using WebApplication1.UOW.IUOW;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.ServiceImpl.Helper
{
    public class OrderFinancialServiceImpl : IOrderFinancialService
    {
        private readonly IAppUnitOfWork _unitOfWork;

        private readonly IBNPL_PlanService _bNPL_PlanService;
        private readonly IBNPL_InstallmentService _bNPL_InstallmentService;
        private readonly IBNPL_PlanSettlementSummaryService _bnplSettlementService;
        private readonly ICashflowService _cashflowService;
        private readonly ILogger<OrderFinancialServiceImpl> _logger;

        public OrderFinancialServiceImpl(
            IAppUnitOfWork unitOfWork,
            IBNPL_PlanService bNPL_PlanService,
            IBNPL_InstallmentService bNPL_InstallmentService,
            IBNPL_PlanSettlementSummaryService bnplSettlementService,
            ICashflowService cashflowService,
            ILogger<OrderFinancialServiceImpl> logger)
        {
            _unitOfWork = unitOfWork;
            _bNPL_PlanService = bNPL_PlanService;
            _bNPL_InstallmentService = bNPL_InstallmentService;
            _bnplSettlementService = bnplSettlementService;
            _cashflowService = cashflowService;
            _logger = logger;
        }

        public async Task BuildPaymentRefundUpdateRequestAsync(CustomerOrder order, DateTime now)
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

        public async Task ApplyOrderPaymentStatusUpdateAsync(int orderId, OrderPaymentStatusEnum newStatus)
        {
            // Optional additional logic if needed
            _logger.LogInformation("Order payment status updated for OrderID={OrderId} to {Status}", orderId, newStatus);
        }
    }
}