using WebApplication1.DTOs.RequestDto.StatusChange;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
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

        private readonly ICustomerService _customerService;
        private readonly IElectronicItemService _electronicItemService;

        //logger: for auditing
        private readonly ILogger<CustomerOrderServiceImpl> _logger;

        // Constructor
        public CustomerOrderServiceImpl(ICustomerOrderRepository repository, IAppUnitOfWork unitOfWork, ICustomerService customerService, IElectronicItemService electronicItemService, ILogger<CustomerOrderServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _unitOfWork = unitOfWork;
            _customerService = customerService;
            _electronicItemService = electronicItemService;
            _logger = logger;
        }

        public async Task<IEnumerable<CustomerOrder>> GetAllCustomerOrdersAsync() =>
            await _repository.GetAllWithCustomerDetailsAsync();

        public async Task<CustomerOrder?> GetCustomerOrderByIdAsync(int id) =>
            await _repository.GetWithCustomerOrderDetailsByIdAsync(id);

        public async Task<CustomerOrder> CreateCustomerOrderWithTransactionAsync(CustomerOrder customerOrder)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(customerOrder.CustomerID)
                    ?? throw new InvalidOperationException($"Customer {customerOrder.CustomerID} not found.");

                var itemIds = customerOrder.CustomerOrderElectronicItems.Select(i => i.E_ItemID).ToList();
                if (itemIds.Count != itemIds.Distinct().Count())
                    throw new InvalidOperationException("Order contains duplicate electronic items.");

                var itemDict = await LoadElectronicItemsAsync(itemIds);

                decimal totalAmount = 0;
                foreach (var orderItem in customerOrder.CustomerOrderElectronicItems)
                {
                    if (!itemDict.TryGetValue(orderItem.E_ItemID, out var electronicItem))
                        throw new InvalidOperationException($"Electronic item {orderItem.E_ItemID} not found.");

                    if (orderItem.Quantity <= 0)
                        throw new InvalidOperationException($"Invalid quantity for item {orderItem.E_ItemID}.");

                    if (electronicItem.QOH < orderItem.Quantity)
                        throw new InvalidOperationException($"Insufficient stock for {electronicItem.ElectronicItemName}");

                    orderItem.UnitPrice = electronicItem.Price;
                    orderItem.SubTotal = orderItem.Quantity * electronicItem.Price;
                    totalAmount += orderItem.SubTotal;

                    // Deduct stock
                    electronicItem.QOH -= orderItem.Quantity;
                }

                customerOrder.TotalAmount = totalAmount;
                customerOrder.OrderDate = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);
                customerOrder.OrderStatus = OrderStatusEnum.Pending;
                customerOrder.OrderPaymentStatus = OrderPaymentStatusEnum.Pending;

                await _repository.AddAsync(customerOrder);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Customer order created: Id={Id}, TotalAmount={Total}", customerOrder.OrderID, totalAmount);
                return customerOrder;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to create customer order.");
                throw;
            }
        }

        //Helper Method : LoadElectronicItemsAsync
        private async Task<Dictionary<int, ElectronicItem>> LoadElectronicItemsAsync(IEnumerable<int> ids)
        {
            var items = await _electronicItemService.GetAllElectronicItemsByIdsAsync(ids.ToList());
            return items.ToDictionary(x => x.ElectronicItemID);
        }

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

                await _repository.UpdateAsync(order.OrderID, order);
                await _unitOfWork.CommitAsync();

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
            // Dictionary defining valid transitions
            var validTransitions = new Dictionary<OrderStatusEnum, List<OrderStatusEnum>>
            {
                { OrderStatusEnum.Pending, new List<OrderStatusEnum> { OrderStatusEnum.Shipped, OrderStatusEnum.Delivered, OrderStatusEnum.Cancel_Pending } },
                { OrderStatusEnum.Shipped, new List<OrderStatusEnum> { OrderStatusEnum.Delivered } },
                { OrderStatusEnum.Cancel_Pending, new List<OrderStatusEnum> { OrderStatusEnum.Cancelled, OrderStatusEnum.DeliveredAfterCancellationRejected } }
            };

            // Add additional validations for special cases
            if (existingOrder.OrderStatus == OrderStatusEnum.Delivered)
            {
                if (newStatus == OrderStatusEnum.Cancel_Pending)
                {
                    var deliveredDate = existingOrder.DeliveredDate ?? now;
                    if ((now - deliveredDate).TotalDays > BnplSystemConstants.FreeTrialPeriodDays)
                        throw new InvalidOperationException("Cannot request cancellation for delivered orders after free trial period.");
                }
                else if (!validTransitions[existingOrder.OrderStatus].Contains(newStatus))
                {
                    throw new InvalidOperationException("Delivered orders can only change to Cancel_Pending within the free trial period.");
                }
            }
            else if (existingOrder.OrderStatus == OrderStatusEnum.Cancelled || existingOrder.OrderStatus == OrderStatusEnum.DeliveredAfterCancellationRejected)
            {
                throw new InvalidOperationException($"{existingOrder.OrderStatus} orders cannot change status.");
            }
            else
            {
                if (!validTransitions.ContainsKey(existingOrder.OrderStatus) || !validTransitions[existingOrder.OrderStatus].Contains(newStatus))
                {
                    throw new InvalidOperationException($"{existingOrder.OrderStatus} orders cannot move to {newStatus}.");
                }
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
                case OrderPaymentStatusEnum.Pending:
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

            // Refund all cashflows
            foreach (var cf in order.Cashflows)
            {
                cf.CashflowStatus = CashflowStatusEnum.Refunded;
                cf.RefundDate = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);
            }

            // Update order payment status to refunded
            order.OrderPaymentStatus = OrderPaymentStatusEnum.Refunded;
        }

        //Helper Method : Cancel partial pay (Bnpl) - refund : cashflow, cancel : Bnpl, Refund : bnpl installemts, Cancel : bnpl_snapshot
        private void HandlePartiallyPaidCancellationAsync(CustomerOrder order)
        {
            HandleRestock(order);

            var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            // Refund cashflows
            foreach (var cf in order.Cashflows)
            {
                if (cf.CashflowStatus != CashflowStatusEnum.Refunded)
                {
                    cf.CashflowStatus = CashflowStatusEnum.Refunded;
                    cf.RefundDate = now;
                }
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
    }
}
