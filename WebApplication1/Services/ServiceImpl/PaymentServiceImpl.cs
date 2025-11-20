using WebApplication1.DTOs.RequestDto.BnplCal;
using WebApplication1.DTOs.RequestDto.Custom;
using WebApplication1.DTOs.RequestDto.Payment;
using WebApplication1.DTOs.RequestDto.StatusChange;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.UOW.IUOW;
using WebApplication1.Utils.Helpers;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.ServiceImpl
{
    public class PaymentServiceImpl : IPaymentService
    {
        private readonly IAppUnitOfWork _unitOfWork;

        //Repositories
        private readonly ICashflowRepository _cashflowRepository;
        private readonly ICustomerOrderRepository _customerOrderRepository;



        //Service
        private readonly ICashflowService _cashflowService;
        private readonly IBNPL_InstallmentService _bNPL_InstallmentService;
        private readonly IBNPL_PlanSettlementSummaryService _bnpl_planSettlementSummaryService;
        private readonly IBNPL_PlanService _bNPL_PlanService;

        //logger: for auditing
        private readonly ILogger<PaymentServiceImpl> _logger;

        // Constructor
        public PaymentServiceImpl(
        IAppUnitOfWork unitOfWork,

        ICashflowRepository cashflowRepository,
        ICustomerOrderRepository customerOrderRepository,


        ICashflowService cashflowService,
        IBNPL_InstallmentService bNPL_InstallmentService, 
        IBNPL_PlanSettlementSummaryService bnpl_planSettlementSummaryService, 
        IBNPL_PlanService bNPL_PlanService,

        ILogger<PaymentServiceImpl> logger)
        {
            // Dependency injection
            _unitOfWork = unitOfWork;

            _cashflowRepository = cashflowRepository;
            _customerOrderRepository = customerOrderRepository;


            _cashflowService = cashflowService;
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

            // Begin UoW-managed transaction
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Starting full payment processing for OrderID={OrderId}, Amount={Amount}",
                    paymentRequest.OrderId, paymentRequest.PaymentAmount);

                // 1. Create a cashflow record
                var cashflow = await _cashflowService.BuildCashflowAddRequestAsync(paymentRequest, CashflowTypeEnum.FullPayment);
                await _cashflowRepository.AddAsync(cashflow);
                _logger.LogInformation("Generated Cashflow record: {CashflowRef}", cashflow.CashflowRef);

                // 2. Update customer order payment status to Fully Paid
                var customerOrderStatusChangeRequest = new CustomerOrderUpdateDto
                {
                    PaymentStatus = OrderPaymentStatusEnum.Fully_Paid
                };

                //var customerOrder = await _customerOrderService.UpdateCustomerOrderPaymentStatusAsync(paymentRequest.OrderId, customerOrderStatusChangeRequest);
                _logger.LogInformation("Updated customer order payment status to Fully Paid for OrderID={OrderId}", paymentRequest.OrderId);

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

                // Create the BNPL plan (no transaction inside)
                var bnpl_plan = await _bNPL_PlanService.AddBNPL_PlanAsync(new BNPL_PLAN
                {
                    Bnpl_InitialPayment = request.InitialPayment,
                    Bnpl_AmountPerInstallment = bnplCalc.AmountPerInstallment,
                    Bnpl_TotalInstallmentCount = request.InstallmentCount,
                    Bnpl_PlanTypeID = request.Bnpl_PlanTypeID,
                    OrderID = request.OrderID,
                });

                // Generate installments
                await _bNPL_InstallmentService.AddBnplInstallmentsAsync(bnpl_plan);

                // Create a cashflow for initial payment
                var paymentRequest = new PaymentRequestDto
                {
                    PaymentAmount = request.InitialPayment,
                    OrderId = request.OrderID
                };

                var cashflow = await _cashflowService.BuildCashflowAddRequestAsync(paymentRequest, CashflowTypeEnum.BnplInitialPayment);
                await _cashflowRepository.AddAsync(cashflow);

                // Generate settlement snapshot
                await _bnpl_planSettlementSummaryService.GenerateSettlementAsync(bnpl_plan.Bnpl_PlanID);

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
        public async Task ProcessBnplInstallmentPaymentAsync(PaymentRequestDto paymentRequest)
        {
            // Begin UoW-managed transaction
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 1. Apply BNPL installment payment
                var paymentResult = await _bNPL_InstallmentService.ApplyBnplInstallmentPaymentAsync(paymentRequest);
                _logger.LogInformation("Applied installment payment: {PaymentResult}", paymentResult);

                // 2. Retrieve associated BNPL plan
                var plan = await _bNPL_PlanService.GetByOrderIdAsync(paymentRequest.OrderId);
                if (plan == null)
                    throw new Exception($"Associated BNPL plan not found for OrderID={paymentRequest.OrderId}");

                // 3. Update BNPL status (plan + order)
                await UpdateBnplPostPaymentStateAsync(plan);

                // 4. Generate settlement snapshot
                var settlementSnapshot = await _bnpl_planSettlementSummaryService.GenerateSettlementAsync(plan.Bnpl_PlanID);
                _logger.LogInformation("Generated settlement snapshot for PlanID={PlanId}", plan.Bnpl_PlanID);

                // 5. Generate cashflow record
                var cashflow = await _cashflowService.BuildCashflowAddRequestAsync(paymentRequest, CashflowTypeEnum.BnplInstallmentPayment);
                await _cashflowRepository.AddAsync(cashflow);
                _logger.LogInformation("Generated Cashflow record: {CashflowRef}", cashflow.CashflowRef);

                // 6. Commit transaction
                await _unitOfWork.CommitAsync();
                _logger.LogInformation("BNPL installment payment processed successfully for OrderID={OrderId}", paymentRequest.OrderId);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to process BNPL installment payment for OrderID={OrderId}", paymentRequest.OrderId);
                throw;
            }
        }

        //Helper Method : update Bnpl plan + customer order
        private async Task UpdateBnplPostPaymentStateAsync(BNPL_PLAN plan)
        {
            if (plan == null)
                throw new ArgumentNullException(nameof(plan));

            var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            // Get installments of this plan
            var installments = await _bNPL_InstallmentService.GetAllByPlanIdAsync(plan.Bnpl_PlanID);

            if (installments == null || !installments.Any())
                throw new Exception($"No installments found for BNPL Plan ID={plan.Bnpl_PlanID}");

            // Define which statuses are considered settled
            var paidStatuses = new[]
            {
                BNPL_Installment_StatusEnum.Paid_OnTime,
                BNPL_Installment_StatusEnum.Paid_Late
            };

            // Remaining installments = not fully paid
            var remainingInstallments = installments!.Count(x => !paidStatuses.Contains(x.Bnpl_Installment_Status));

            plan.Bnpl_RemainingInstallmentCount = remainingInstallments;

            // Next installment due date (if any)
            plan.Bnpl_NextDueDate = installments!
                .Where(x => !paidStatuses.Contains(x.Bnpl_Installment_Status))
                .OrderBy(x => x.Installment_DueDate)
                .Select(x => x.Installment_DueDate)
                .FirstOrDefault();

            // If all installments paid : mark completed
            if (remainingInstallments == 0)
            {
                plan.CompletedAt = now;
                plan.Bnpl_Status = BnplStatusEnum.Completed;
            }

            // Update plan (only changed fields)
            await _bNPL_PlanService.UpdateBNPL_PlanAsync(plan.Bnpl_PlanID, plan);

            // update customer order based on new state
            var request = new CustomerOrderPaymentStatusChangeRequestDto
            {
                OrderID = plan.OrderID,
                NewPaymentStatus = OrderPaymentStatusEnum.Partially_Paid,
            };

            await UpdateCustomerOrderPaymentStatusAsync(request);
        }

        //Helper Method
        private async Task<CustomerOrder?> UpdateCustomerOrderPaymentStatusAsync(CustomerOrderPaymentStatusChangeRequestDto request)
        {
            var order = await _customerOrderRepository.GetByIdAsync(request.OrderID);
            if (order == null)
                throw new Exception("Customer order not found");

            var oldStatus = order.OrderPaymentStatus;

            // No change
            if (oldStatus == request.NewPaymentStatus)
                return order;

            // Validate allowed transitions
            switch (oldStatus)
            {
                case OrderPaymentStatusEnum.Partially_Paid:
                    if (request.NewPaymentStatus != OrderPaymentStatusEnum.Fully_Paid &&
                        request.NewPaymentStatus != OrderPaymentStatusEnum.Overdue)
                        throw new InvalidOperationException(
                            "Partially paid orders can only move to 'Fully_Paid', or 'Overdue'.");
                    break;

                case OrderPaymentStatusEnum.Fully_Paid:
                    if (request.NewPaymentStatus != OrderPaymentStatusEnum.Refunded)
                        throw new InvalidOperationException(
                            "Fully paid orders can only move to 'Refunded'.");
                    break;

                case OrderPaymentStatusEnum.Overdue:
                    if (request.NewPaymentStatus != OrderPaymentStatusEnum.Fully_Paid &&
                        request.NewPaymentStatus != OrderPaymentStatusEnum.Partially_Paid)
                        throw new InvalidOperationException(
                            "Overdue orders can only move to 'Partially_Paid' or 'Fully_Paid'.");
                    break;

                case OrderPaymentStatusEnum.Refunded:
                    throw new InvalidOperationException(
                        "Refunded orders cannot change payment status.");
            }

            // Check total paid so far
            var totalPaid = await _cashflowRepository.SumCashflowsByOrderAsync(request.OrderID);

            // If payment is now complete, we override with Fully Paid
            if (totalPaid >= order.TotalAmount)
            {
                order.PaymentCompletedDate = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);
                order.OrderPaymentStatus = OrderPaymentStatusEnum.Fully_Paid;
            }
            else
            {
                // Otherwise use requested status
                order.OrderPaymentStatus = request.NewPaymentStatus;
            }

            await _customerOrderRepository.UpdateAsync(request.OrderID, order);
            
            _logger.LogInformation(
                "Customer payment status updated: Id={Id}, PaymentStatus={PaymentStatus}",
                order.OrderID, order.OrderPaymentStatus);

            return order;
        }
    }
}