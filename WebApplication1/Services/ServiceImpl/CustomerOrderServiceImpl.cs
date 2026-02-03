using AutoMapper;
using WebApplication1.DTOs.RequestDto;
using WebApplication1.DTOs.RequestDto.BnplCal;
using WebApplication1.DTOs.RequestDto.StatusChange;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.Services.IService.Auth;
using WebApplication1.UOW.IUOW;
using WebApplication1.Utils.Helpers;
using WebApplication1.Utils.Project_Enums;
using WebApplication1.Utils.SystemConstants;

namespace WebApplication1.Services.ServiceImpl
{
    public class CustomerOrderServiceImpl : ICustomerOrderService
    {
        private readonly ICustomerOrderRepository _repository;
        private readonly IAppUnitOfWork _unitOfWork;

        private readonly IUserRepository _userRepository;

        private readonly ICurrentUserService _currentUserService;
        private readonly ICustomerService _customerService;
        private readonly IElectronicItemService _electronicItemService;
        private readonly IPhysicalShopSessionService _physicalShopSessionService;
        private readonly IBNPL_InstallmentService _bNPL_InstallmentService;
        private readonly IBNPL_PlanSettlementSummaryService _bnpl_planSettlementSummaryService;
        private readonly IBNPL_PlanService _bNPL_PlanService;
        private readonly IInvoiceService _invoiceService;
        private readonly ICashflowService _cashflowService;

        //logger: for auditing
        private readonly ILogger<CustomerOrderServiceImpl> _logger;
        private readonly IMapper _mapper;

        // Constructor
        public CustomerOrderServiceImpl(
            ICustomerOrderRepository repository,
            IAppUnitOfWork unitOfWork,
            IUserRepository userRepository,

            ICurrentUserService currentUserService,
            ICustomerService customerService,
            IElectronicItemService electronicItemService,
            IPhysicalShopSessionService physicalShopSessionService,
            IBNPL_InstallmentService bNPL_InstallmentService,
            IBNPL_PlanSettlementSummaryService bnpl_planSettlementSummaryService,
            IBNPL_PlanService bNPL_PlanService,
            ILogger<CustomerOrderServiceImpl> logger,
            IInvoiceService invoiceService,
            ICashflowService cashflowService,
            IMapper mapper)
        {
            // Dependency injection
            _repository = repository;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;

            _currentUserService = currentUserService;
            _customerService = customerService;
            _electronicItemService = electronicItemService;
            _physicalShopSessionService = physicalShopSessionService;
            _bNPL_InstallmentService = bNPL_InstallmentService;
            _bnpl_planSettlementSummaryService = bnpl_planSettlementSummaryService;
            _bNPL_PlanService = bNPL_PlanService;
            _invoiceService = invoiceService;
            _cashflowService = cashflowService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CustomerOrder>> GetAllCustomerOrdersAsync() =>
            await _repository.GetAllWithCustomerDetailsAsync();

        public async Task<CustomerOrder?> GetCustomerOrderByIdAsync(int id) =>
            await _repository.GetWithCustomerOrderDetailsByIdAsync(id);



        // -------- [Start: create an order + bnpl_plan + bnpl_installments + initial bnpl_snapshot] -------- 
        public async Task<CustomerOrder> CreateCustomerOrderWithTransactionAsync(CustomerOrderRequestDto createRequest)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                int? orderCustomerID = null;

                // ------------------------------------
                // ONLINE SHOP ORDER (Customer only)
                // ------------------------------------
                if (createRequest.OrderSource == OrderSourceEnum.OnlineShop)
                {
                    if (_currentUserService.Role != UserRoleEnum.Customer.ToString())
                        throw new UnauthorizedAccessException(
                            "Only customers can place online orders.");

                    var userId = _currentUserService.UserID
                        ?? throw new UnauthorizedAccessException("User not authenticated");

                    var user = await _userRepository
                        .GetWithRoleProfileDetailsByIdAsync(userId)
                        ?? throw new UnauthorizedAccessException("User not found");

                    if (user.Customer == null)
                        throw new InvalidOperationException("Customer profile not found");

                    orderCustomerID = user.Customer.CustomerID;
                }

                // ------------------------------------
                // PHYSICAL SHOP ORDER (Manager)
                // ------------------------------------
                else if (createRequest.OrderSource == OrderSourceEnum.PhysicalShop)
                {
                    if (createRequest.PhysicalShopBillToCustomerID.HasValue)
                    {
                        var customerExists = await _customerService.GetCustomerByIdAsync(createRequest.PhysicalShopBillToCustomerID.Value);

                        if (customerExists == null)
                            throw new InvalidOperationException("Selected customer does not exist.");

                        orderCustomerID = createRequest.PhysicalShopBillToCustomerID.Value;
                    }
                }

                // =====================================================
                // Business Rules
                // =====================================================
                // BNPL requires registered customer
                if (createRequest.OrderPaymentMode == OrderPaymentModeEnum.Pay_Bnpl
                    && !orderCustomerID.HasValue)
                {
                    throw new InvalidOperationException(
                        "BNPL orders require a registered customer.");
                }

                // Prevent multiple pending orders per customer
                if (orderCustomerID.HasValue)
                {
                    bool hasPending = await _repository.ExistsPendingOrderForCustomerAsync(orderCustomerID.Value);

                    if (hasPending)
                        throw new InvalidOperationException("Customer already has a pending order. Please complete or cancel it before placing a new order.");
                }

                // =====================================================
                // Map Request -> Entity
                // =====================================================
                var customerOrder = _mapper.Map<CustomerOrder>(createRequest);

                // Who the order belongs to
                customerOrder.CustomerID = orderCustomerID;

                // Who created the order (audit)
                customerOrder.CreatedByUserID = _currentUserService.UserID ?? throw new UnauthorizedAccessException("User not authenticated");

                // =====================================================
                // Physical Shop Session Validation
                // =====================================================
                if (createRequest.OrderSource == OrderSourceEnum.PhysicalShop)
                {
                    var activeSession = await _physicalShopSessionService
                        .GetLatestActivePhysicalShopSessionAsync();

                    if (activeSession == null)
                        throw new InvalidOperationException(
                            "No active physical shop session found. Please open a shop session first.");

                    // Attach active session
                    customerOrder.PhysicalShopSessionId =
                        activeSession.PhysicalShopSessionID;
                }
                else
                {
                    // Online orders must not have a physical session
                    customerOrder.PhysicalShopSessionId = null;
                }

                // =====================================================
                // Build Items, Stock & Totals
                // =====================================================
                await BuildOrderItemsAndTotalAsync(customerOrder);

                // =====================================================
                // Order Metadata
                // =====================================================
                customerOrder.OrderDate = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);
                customerOrder.OrderStatus = OrderStatusEnum.Pending;
                customerOrder.OrderPaymentStatus = OrderPaymentStatusEnum.Awaiting_Payment;
                ApplyOnlineOrderAutoCancellation(customerOrder);

                // =====================================================
                // Persist Order (Generate OrderID)
                // =====================================================
                await _repository.AddAsync(customerOrder);
                await _unitOfWork.SaveChangesAsync();

                // =====================================================
                // Handle Payment (BNPL / Full)
                // =====================================================
                await HandleOrderPaymentAsync(customerOrder, createRequest);

                // =====================================================
                // Invoice Creation (Draft)
                // =====================================================
                if (customerOrder.OrderPaymentMode == OrderPaymentModeEnum.Pay_now_full)
                {
                    var invoice = await _invoiceService.BuildInvoiceAddRequestAsync(customerOrder, InvoiceTypeEnum.Full_Pay);
                    customerOrder.Invoices.Add(invoice);
                }
                else
                {
                    var invoice = await _invoiceService.BuildInvoiceAddRequestAsync(customerOrder, InvoiceTypeEnum.Bnpl_Initial_Pay);
                    customerOrder.Invoices.Add(invoice);
                }

                await _unitOfWork.SaveChangesAsync();

                // =====================================================
                // Audit Log (After OrderID exists)
                // =====================================================
                LogOrderCreation(customerOrder);

                await _unitOfWork.CommitAsync();

                return customerOrder;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to create customer order.");
                throw;
            }
        }

        //Helper Method : BuildOrderItemsAndTotalAsync
        private async Task BuildOrderItemsAndTotalAsync(CustomerOrder customerOrder)
        {
            var itemIds =
                customerOrder.CustomerOrderElectronicItems
                    .Select(i => i.E_ItemID)
                    .ToList();

            if (itemIds.Count != itemIds.Distinct().Count())
                throw new InvalidOperationException("Order contains duplicate electronic items.");

            var itemDict = await LoadElectronicItemsAsync(itemIds);

            decimal totalAmount = 0;

            foreach (var orderItem in customerOrder.CustomerOrderElectronicItems)
            {
                if (!itemDict.TryGetValue(orderItem.E_ItemID, out var electronicItem))
                    throw new InvalidOperationException(
                        $"Electronic item {orderItem.E_ItemID} not found.");

                if (orderItem.Quantity <= 0)
                    throw new InvalidOperationException(
                        $"Invalid quantity for item {orderItem.E_ItemID}.");

                if (electronicItem.QOH < orderItem.Quantity)
                    throw new InvalidOperationException(
                        $"Insufficient stock for {electronicItem.ElectronicItemName}");

                orderItem.UnitPrice = electronicItem.Price;
                orderItem.SubTotal = orderItem.Quantity * electronicItem.Price;
                totalAmount += orderItem.SubTotal;

                // Deduct stock
                electronicItem.QOH -= orderItem.Quantity;
            }

            customerOrder.TotalAmount = totalAmount;
        }

        //Helper Method : LoadElectronicItemsAsync
        private async Task<Dictionary<int, ElectronicItem>> LoadElectronicItemsAsync(IEnumerable<int> ids)
        {
            var items = await _electronicItemService.GetAllElectronicItemsByIdsAsync(ids.ToList());
            return items.ToDictionary(x => x.ElectronicItemID);
        }

        //Helper Method : HandlePaymentAsync
        private async Task HandleOrderPaymentAsync(CustomerOrder order, CustomerOrderRequestDto request)
        {
            bool isBnpl =
                request.Bnpl_PlanTypeID.HasValue &&
                request.Bnpl_InstallmentCount.HasValue &&
                request.Bnpl_InitialPayment.HasValue;

            if (isBnpl)
            {
                await HandleBnplOrderAsync(order, request);
            }
            else
            {
                HandleFullPaymentOrder(order);
            }
        }

        //Helper Method : HandleFullPaymentOrder
        private void HandleFullPaymentOrder(CustomerOrder order)
        {
            order.OrderPaymentMode = OrderPaymentModeEnum.Pay_now_full;
            order.BNPL_PLAN = null;
        }

        //Helper Method : HandleBnplOrderAsync
        private async Task HandleBnplOrderAsync(CustomerOrder order, CustomerOrderRequestDto request)
        {
            if (request.Bnpl_InitialPayment <= 0)
                throw new InvalidOperationException(
                    "Initial payment must be greater than zero.");

            if (request.Bnpl_InitialPayment >= order.TotalAmount)
                throw new InvalidOperationException(
                    "Initial payment must be less than total amount.");

            var bnplCalc =
                await _bNPL_PlanService
                    .CalculateBNPL_PlanAmountPerInstallmentAsync(
                        new BNPLInstallmentCalculatorRequestDto
                        {
                            TotalOrderAmount = order.TotalAmount,
                            InitialPayment = request.Bnpl_InitialPayment!.Value,
                            Bnpl_PlanTypeID = request.Bnpl_PlanTypeID!.Value,
                            InstallmentCount = request.Bnpl_InstallmentCount!.Value
                        });

            var bnplPlan =
                await _bNPL_PlanService
                    .BuildBnpl_PlanAddRequestAsync(
                        new BNPL_PLAN
                        {
                            OrderID = order.OrderID,
                            Bnpl_InitialPayment = request.Bnpl_InitialPayment.Value,
                            Bnpl_AmountPerInstallment = bnplCalc.AmountPerInstallment,
                            Bnpl_TotalInstallmentCount = request.Bnpl_InstallmentCount.Value,
                            Bnpl_PlanTypeID = request.Bnpl_PlanTypeID.Value,
                            Bnpl_Status = BnplStatusEnum.Requested
                        });

            var installments =
                await _bNPL_InstallmentService
                    .BuildBnplInstallmentBulkAddRequestAsync(bnplPlan);

            foreach (var inst in installments)
                bnplPlan.BNPL_Installments.Add(inst);

            var snapshot =
                _bnpl_planSettlementSummaryService
                    .BuildSettlementGenerateRequestForPlan(bnplPlan);

            if (snapshot != null)
                bnplPlan.BNPL_PlanSettlementSummaries.Add(snapshot);

            order.BNPL_PLAN = bnplPlan;
            order.OrderPaymentMode = OrderPaymentModeEnum.Pay_Bnpl;
        }

        //Helper Method: ApplyOnlineOrderAutoCancellation
        private void ApplyOnlineOrderAutoCancellation(CustomerOrder order)
        {
            if (order.OrderSource == OrderSourceEnum.OnlineShop)
            {
                order.PendingPaymentOrderAutoCancelledDate =
                    order.OrderDate.AddMinutes(5);
            }
        }

        //Helper Method: LogOrderCreation
        private void LogOrderCreation(CustomerOrder order)
        {
            if (order.CustomerID.HasValue)
            {
                _logger.LogInformation(
                    "Customer order created for customer: Id={CustomerId}, OrderId={OrderId}, TotalAmount={Total}",
                    order.CustomerID.Value,
                    order.OrderID,
                    order.TotalAmount);
            }
            else
            {
                _logger.LogInformation(
                    "Direct order created: OrderId={OrderId}, TotalAmount={Total}",
                    order.OrderID,
                    order.TotalAmount);
            }
        }
        // -------- [End: create an order + bnpl_plan + bnpl_installments + initial bnpl_snapshot] -------- 

        //Custom Query Operations
        public async Task<CustomerOrder?> GetCustomerOrderWithFinancialDetailsByIdAsync(int id) =>
            await _repository.GetWithFinancialDetailsByIdAsync(id);

        public async Task<PaginationResultDto<CustomerOrder>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? paymentStatusId = null, int? orderStatusId = null, string? searchKey = null) =>
            await _repository.GetAllWithPaginationAsync(pageNumber, pageSize, paymentStatusId, orderStatusId, searchKey);

        public async Task<PaginationResultDto<CustomerOrder>> GetAllByCustomerWithPaginationAsync(int customerId, int pageNumber, int pageSize, int? orderStatusId = null, string? searchKey = null) =>
            await _repository.GetAllByCustomerWithPaginationAsync(customerId, pageNumber, pageSize, orderStatusId, searchKey);

        // Update : Order Status
        public async Task<CustomerOrder?> ModifyCustomerOrderStatusWithTransactionAsync(int orderId, CustomerOrderStatusChangeRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            Cashflow[] refundCashflows = Array.Empty<Cashflow>();

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var order = await _repository.GetWithFinancialDetailsByIdAsync(orderId)
                    ?? throw new InvalidOperationException("Customer order not found");

                if (order.OrderStatus == request.NewOrderStatus)
                    return order;

                //validation
                ValidateOrderStatusTransition(order, request.NewOrderStatus, now);

                ApplyOrderStatusChangesAsync(order, request, now);

                // collect refund cashflows BEFORE commit
                if (request.NewOrderStatus == OrderStatusEnum.Cancelled)
                {
                    refundCashflows = GetRefundCashflows(order).ToArray();
                }

                await _repository.UpdateAsync(order.OrderID, order);
                await _unitOfWork.CommitAsync();

                // AFTER COMMIT -> generate refund receipts
                foreach (var cashflow in refundCashflows)
                {
                    await _cashflowService.GenerateCashflowReceiptAsync(cashflow.CashflowID);
                }

                _logger.LogInformation("Customer order status updated: Id={Id}, Status={Status}", order.OrderID, order.OrderStatus);
                return order;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to update customer order status for OrderID={OrderID}", orderId);
                throw;
            }
        }

        //Helper Method : ValidateOrderStatusTransition
        private void ValidateOrderStatusTransition(CustomerOrder existingOrder, OrderStatusEnum newStatus, DateTime now)
        {
            if (existingOrder == null)
                throw new ArgumentNullException(nameof(existingOrder));

            var currentStatus = existingOrder.OrderStatus;

            // Dictionary defining valid transitions
            var validTransitions = new Dictionary<OrderStatusEnum, HashSet<OrderStatusEnum>>
            {
                {
                    OrderStatusEnum.Pending,
                    new HashSet<OrderStatusEnum>
                    {
                        OrderStatusEnum.Processing,
                        OrderStatusEnum.Cancel_Pending
                    }
                },
                {
                    OrderStatusEnum.Processing,
                    new HashSet<OrderStatusEnum>
                    {
                        OrderStatusEnum.Shipped,
                        OrderStatusEnum.Cancel_Pending
                    }
                },
                {
                    OrderStatusEnum.Shipped,
                    new HashSet<OrderStatusEnum>
                    {
                        OrderStatusEnum.Delivered
                    }
                },
                {
                    OrderStatusEnum.Delivered,
                    new HashSet<OrderStatusEnum>
                    {
                        OrderStatusEnum.Cancel_Pending
                    }
                },
                {
                    OrderStatusEnum.Cancel_Pending,
                    new HashSet<OrderStatusEnum>
                    {
                        OrderStatusEnum.Cancelled,
                        OrderStatusEnum.DeliveredAfterCancellationRejected
                    }
                }
            };
            
            // Final states: no transitions allowed
            if (currentStatus == OrderStatusEnum.Cancelled || currentStatus == OrderStatusEnum.DeliveredAfterCancellationRejected)
            {
                throw new InvalidOperationException(
                    $"{currentStatus} orders cannot change status."
                );
            }

            // Delivered → Cancel_Pending only within free trial period
            if (currentStatus == OrderStatusEnum.Delivered && newStatus == OrderStatusEnum.Cancel_Pending)
            {
                var deliveredDate = existingOrder.DeliveredDate ?? now;

                if ((now - deliveredDate).TotalDays > BnplSystemConstants.FreeTrialPeriodDays)
                {
                    throw new InvalidOperationException(
                        "Cannot request cancellation for delivered orders after the free trial period."
                    );
                }

                return; // valid transition
            }

            // Generic transition validation
            if (!validTransitions.TryGetValue(currentStatus, out var allowedNextStatuses) || !allowedNextStatuses.Contains(newStatus))
            {
                throw new InvalidOperationException(
                    $"{currentStatus} orders cannot move to {newStatus}."
                );
            }
        }

        // Helper Method: Applies status changes to the order
        private void ApplyOrderStatusChangesAsync(CustomerOrder order, CustomerOrderStatusChangeRequestDto request, DateTime now)
        {
            switch (request.NewOrderStatus)
            {
                case OrderStatusEnum.Cancel_Pending:
                    order.OrderStatus = OrderStatusEnum.Cancel_Pending;
                    order.CancellationReason = request.CancellationReason;
                    order.CancellationRequestDate = now;
                    order.CancellationApproved = null; // pending
                    break;

                case OrderStatusEnum.Cancelled:
                    order.OrderStatus = OrderStatusEnum.Cancelled;
                    order.CancelledDate = now;
                    order.CancellationApproved = true;
                    HandleCancelOrderAsync(order);
                    break;

                case OrderStatusEnum.Shipped:
                    order.OrderStatus = OrderStatusEnum.Shipped;
                    order.ShippedDate = now;
                    break;

                case OrderStatusEnum.Delivered:
                    order.OrderStatus = OrderStatusEnum.Delivered;
                    order.DeliveredDate = now;
                    break;

                case OrderStatusEnum.DeliveredAfterCancellationRejected:
                    order.OrderStatus = OrderStatusEnum.DeliveredAfterCancellationRejected;
                    order.CancellationRejectionReason = request.CancellationRejectionReason;
                    order.DeliveredDate = now;
                    order.CancellationApproved = false;
                    order.CancellationRejectionReason ??= "Cancellation rejected";
                    break;
            }
        }

        //Helper Method : CancelOrder
        private void HandleCancelOrderAsync(CustomerOrder order)
        {
            switch (order.OrderPaymentStatus)
            {
                case OrderPaymentStatusEnum.Awaiting_Payment:
                    // No payments - just cancel order
                    HandleRestock(order);
                    break;

                case OrderPaymentStatusEnum.Fully_Paid:
                    // Full pay (no Bnpl) - refund : cashflow
                    HandleFullyPaidCancellationAsync(order);
                    break;

                case OrderPaymentStatusEnum.Partially_Paid:
                    // Partial pay (Bnpl) - refund : cashflow, cancel : Bnpl, Refund : bnpl installemts, Cancel : bnpl_snapshot
                    HandlePartiallyPaidCancellationAsync(order);
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported payment status for cancellation: {order.OrderPaymentStatus}");
            }
        }

        //Helper Method : Cancel full pay (no Bnpl) - refund : cashflow
        private void HandleFullyPaidCancellationAsync(CustomerOrder order)
        {
            HandleRestock(order);

            var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            foreach (var invoice in order.Invoices
                         .Where(i => i.InvoiceStatus == InvoiceStatusEnum.Paid))
            {
                // Calculate total paid for this invoice
                var paidAmount = invoice.Cashflows
                    .Where(c => c.CashflowPaymentNature == CashflowPaymentNatureEnum.Payment)
                    .Sum(c => c.AmountPaid);

                if (paidAmount <= 0)
                    continue;

                // Add REFUND cashflow (append-only)
                invoice.Cashflows.Add(new Cashflow
                {
                    AmountPaid = -paidAmount,
                    CashflowRef = $"CF-{invoice.InvoiceID}-{CashflowPaymentNatureEnum.Refund}-{now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..6]}",
                    CashflowDate = now,
                    CashflowPaymentNature = CashflowPaymentNatureEnum.Refund,
                });

                // Update invoice status
                invoice.InvoiceStatus = InvoiceStatusEnum.Refunded;
            }

            // Update order payment status
            order.OrderPaymentStatus = OrderPaymentStatusEnum.Refunded;
        }

        //Helper Method : Cancel partial pay (Bnpl) - refund : cashflow, cancel : Bnpl, Refund : bnpl installemts, Cancel : bnpl_snapshot
        private void HandlePartiallyPaidCancellationAsync(CustomerOrder order)
        {
            HandleRestock(order);

            var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            // Refund all PAID invoices (BNPL installments + initial)
            foreach (var invoice in order.Invoices
                         .Where(i => i.InvoiceStatus == InvoiceStatusEnum.Paid))
            {
                var paidAmount = invoice.Cashflows
                    .Where(c => c.CashflowPaymentNature == CashflowPaymentNatureEnum.Payment)
                    .Sum(c => c.AmountPaid);

                if (paidAmount <= 0)
                    continue;

                invoice.Cashflows.Add(new Cashflow
                {
                    AmountPaid = -paidAmount,
                    CashflowRef = $"CF-{invoice.InvoiceID}-{CashflowPaymentNatureEnum.Refund}-{now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..6]}",
                    CashflowDate = now,
                    CashflowPaymentNature = CashflowPaymentNatureEnum.Refund,
                });

                invoice.InvoiceStatus = InvoiceStatusEnum.Refunded;
            }

            // Cancel BNPL plan
            if (order.BNPL_PLAN != null)
            {
                //Bnpl plan
                order.BNPL_PLAN.Bnpl_Status = BnplStatusEnum.Cancelled;
                order.BNPL_PLAN.CancelledAt = now;

                // Mark installments as refunded
                foreach (var inst in order.BNPL_PLAN.BNPL_Installments)
                {
                    inst.Bnpl_Installment_Status = BNPL_Installment_StatusEnum.Refunded;
                    inst.RefundDate = now;
                }

                // Cancel settlement snapshots
                foreach (var summary in order.BNPL_PLAN.BNPL_PlanSettlementSummaries)
                {
                    summary.Bnpl_PlanSettlementSummary_Status = BNPL_PlanSettlementSummary_StatusEnum.Cancelled;
                    summary.IsLatest = false;
                }
            }

            // Update order payment status to refunded
            order.OrderPaymentStatus = OrderPaymentStatusEnum.Refunded;
        }

        //Helper Method : Restock
        private void HandleRestock(CustomerOrder order)
        {
            foreach (var item in order.CustomerOrderElectronicItems)
                item.ElectronicItem.QOH += item.Quantity;
        }

        //Helper method: Get Refund Cashflows
        private static IEnumerable<Cashflow> GetRefundCashflows(CustomerOrder order)
        {
            return order.Invoices
                .SelectMany(i => i.Cashflows)
                .Where(c => c.CashflowPaymentNature == CashflowPaymentNatureEnum.Refund);
        }

        public async Task<CustomerOrder?> GetCustomerOrderWithActiveBnplByIdAsync(int id, int? customerId = null) =>
            await _repository.GetActiveBnplByIdAsync(id, customerId);

        public async Task<IEnumerable<CustomerOrder>> GetAllActiveBnplCustomerOrdersByCustomerIdAsync(int customerId) =>
            await _repository.GetAllActiveBnplByCustomerIdAsync(customerId);

        public async Task AutoCancelExpiredOnlineOrdersAsync()
        {
            var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var orders =
                    await _repository.GetExpiredPendingOnlineOrdersAsync(now);

                if (!orders.Any())
                    return;

                foreach (var order in orders)
                {
                    order.OrderStatus = OrderStatusEnum.Cancelled;
                    order.CancelledDate = now;
                    order.CancellationReason = "Auto-cancelled due to payment timeout";
                    order.CancellationApproved = true;

                    // restore stock
                    foreach (var item in order.CustomerOrderElectronicItems)
                    {
                        item.ElectronicItem.QOH += item.Quantity;
                    }

                    _logger.LogInformation("Order auto-cancelled: OrderId={OrderId}", order.OrderID);
                }

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}
