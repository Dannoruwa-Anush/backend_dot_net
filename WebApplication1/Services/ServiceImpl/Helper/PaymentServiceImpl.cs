using WebApplication1.DTOs.RequestDto.BnplCal;
using WebApplication1.DTOs.RequestDto.Payment;
using WebApplication1.DTOs.ResponseDto.Payment.Bnpl;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
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

        //Repositories
        private readonly IBNPL_InstallmentRepository _bNPL_InstallmentRepository;



        //Service
        private readonly ICashflowService _cashflowService;
        private readonly IBNPL_InstallmentService _bNPL_InstallmentService;
        private readonly IBNPL_PlanSettlementSummaryService _bnpl_planSettlementSummaryService;
        private readonly IBNPL_PlanService _bNPL_PlanService;
        private readonly IOrderFinancialService _orderFinancialService;

        //logger: for auditing
        private readonly ILogger<PaymentServiceImpl> _logger;

        // Constructor
        public PaymentServiceImpl(
        IAppUnitOfWork unitOfWork,
        IBNPL_InstallmentRepository bNPL_InstallmentRepository,

        ICashflowService cashflowService,
        IBNPL_InstallmentService bNPL_InstallmentService,
        IBNPL_PlanSettlementSummaryService bnpl_planSettlementSummaryService,
        IBNPL_PlanService bNPL_PlanService,
        IOrderFinancialService orderFinancialService,
        ILogger<PaymentServiceImpl> logger)
        {
            // Dependency injection
            _unitOfWork = unitOfWork;
            _bNPL_InstallmentRepository = bNPL_InstallmentRepository;

            _cashflowService = cashflowService;
            _bNPL_InstallmentService = bNPL_InstallmentService;
            _bnpl_planSettlementSummaryService = bnpl_planSettlementSummaryService;
            _bNPL_PlanService = bNPL_PlanService;
            _orderFinancialService = orderFinancialService;
            _logger = logger;
        }

        // Full Payment
        public async Task ProcessFullPaymentPaymentAsync(PaymentRequestDto paymentRequest)
        {
            if (paymentRequest == null)
                throw new ArgumentNullException(nameof(paymentRequest));

            // Begin UoW-managed transaction
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 1. Create a cashflow record
                var cashflow = await _cashflowService.BuildCashflowAddRequestAsync(paymentRequest, CashflowTypeEnum.FullPayment);
              
                // 2. Update customer order payment status to Fully Paid
                //var updatedOrder = await _financialService.ApplyOrderPaymentStatusUpdateAsync(new CustomerOrderPaymentStatusChangeRequestDto { OrderID = paymentRequest.OrderId, NewPaymentStatus = OrderPaymentStatusEnum.Fully_Paid });
               
                // 3. Commit the transaction
                await _unitOfWork.CommitAsync();
                _logger.LogInformation("Full payment processed successfully for OrderID={OrderId}", paymentRequest.OrderId);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to process full payment for OrderID={OrderId}", paymentRequest.OrderId);
                throw;
            }
        }

        // BNPL : Initial Payment (After the initial payment is completed, bnpl plan will be created)
        public async Task ProcessBnplInitialPaymentAsync(BNPLInstallmentCalculatorRequestDto request)
        {
            // Begin UoW-managed transaction
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

                // Calculate BNPL values
                var bnplCalc = await _bNPL_PlanService.CalculateBNPL_PlanAmountPerInstallmentAsync(request);

                // Create the BNPL plan 
                var bnpl_plan = await _bNPL_PlanService.BuildBnpl_PlanAddRequestAsync(new BNPL_PLAN
                {
                    Bnpl_InitialPayment = request.InitialPayment,
                    Bnpl_AmountPerInstallment = bnplCalc.AmountPerInstallment,
                    Bnpl_TotalInstallmentCount = request.InstallmentCount,
                    Bnpl_PlanTypeID = request.Bnpl_PlanTypeID,
                    OrderID = request.OrderID,
                });

                // Generate installments
                var installments = await _bNPL_InstallmentService.BuildBnplInstallmentBulkAddRequestAsync(bnpl_plan);

                // Create a cashflow for initial payment
                var cashflow = await _cashflowService.BuildCashflowAddRequestAsync(new PaymentRequestDto
                {
                    PaymentAmount = request.InitialPayment,
                    OrderId = request.OrderID
                }, CashflowTypeEnum.BnplInitialPayment);

                // Generate settlement snapshot
                var snapshot = await _bnpl_planSettlementSummaryService.BuildSettlementGenerateRequestAsync(bnpl_plan.Bnpl_PlanID);
               
                // Commit
                await _unitOfWork.CommitAsync();
                _logger.LogInformation("BNPL initial payment processed successfully for OrderID={OrderId}", request.OrderID);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to process BNPL initial payment for OrderID={OrderId}", request.OrderID);
                throw;
            }
        }

        // BNPL : Installment Payment
        public async Task<BnplInstallmentPaymentResultDto> ProcessBnplInstallmentPaymentAsync(PaymentRequestDto paymentRequest)
        {
            // Begin UoW-managed transaction
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 1. Apply BNPL installment payment
                var (paymentResult, updatedInstallments) = await _bNPL_InstallmentService.BuildBnplInstallmentSettlementAsync(paymentRequest);
                await _bNPL_InstallmentRepository.UpdateRangeAsync(updatedInstallments);
                _logger.LogInformation("Applied installment payment: {PaymentResult}", paymentResult);

                // 2. Retrieve associated BNPL plan
                var plan = await _bNPL_PlanService.GetByOrderIdAsync(paymentRequest.OrderId);
                if (plan == null)
                    throw new Exception($"Associated BNPL plan not found for OrderID={paymentRequest.OrderId}");

                // 3. Update BNPL status (plan + order)
                //await UpdateBnplPostPaymentStateAsync(plan);

                // 4. Generate settlement snapshot
                var settlementSnapshot = await _bnpl_planSettlementSummaryService.BuildSettlementGenerateRequestAsync(plan.Bnpl_PlanID);

                // 5. Generate cashflow record
                var cashflow = await _cashflowService.BuildCashflowAddRequestAsync(paymentRequest, CashflowTypeEnum.BnplInstallmentPayment);

                // 6. Commit transaction
                await _unitOfWork.CommitAsync();
                _logger.LogInformation("BNPL installment payment processed successfully for OrderID={OrderId}", paymentRequest.OrderId);

                return paymentResult;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to process BNPL installment payment for OrderID={OrderId}", paymentRequest.OrderId);
                throw;
            }
        }

        public async Task BuildPaymentRefundUpdateRequestAsync(CustomerOrder order, DateTime now)
        {   
            //Cashflow : refunds
            await _cashflowService.BuildCashflowStatusUpdateRequestAsync(order, CashflowStatusEnum.Refunded, now);

            //BNPL_Plan : Cancel
            await _bNPL_PlanService.BuildBnplPlanStatusUpdateRequestAsync(order.BNPL_PLAN!, BnplStatusEnum.Cancelled, now);

            // Installment : Refund
            await _bNPL_InstallmentService.BuildBnplInstallmetStatusUpdateRequestAsync(order.BNPL_PLAN!.BNPL_Installments, BNPL_Installment_StatusEnum.Refunded, now);

            // Snapshot : Cancelled
            await _bnpl_planSettlementSummaryService.BuildBnplSettlementSummaryStatusUpdateRequestAsync(order.BNPL_PLAN!.BNPL_PlanSettlementSummaries, BNPL_PlanSettlementSummary_StatusEnum.Cancelled, now);
        }
    }
}