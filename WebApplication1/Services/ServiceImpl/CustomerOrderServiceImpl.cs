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

        private readonly ICustomerRepository _customerRepository;
        private readonly IElectronicItemRepository _electronicItemRepository;
        private readonly ICashflowRepository _cashflowRepository;

        //logger: for auditing
        private readonly ILogger<CustomerOrderServiceImpl> _logger;

        // Constructor
        public CustomerOrderServiceImpl(ICustomerOrderRepository repository, IAppUnitOfWork unitOfWork, ICustomerRepository customerRepository, ICashflowRepository cashflowRepository, IElectronicItemRepository electronicItemRepository, ILogger<CustomerOrderServiceImpl> logger)
        {
            // Dependency injection
            _repository               = repository;
            _unitOfWork               = unitOfWork;
            _customerRepository       = customerRepository;
            _electronicItemRepository = electronicItemRepository;
            _cashflowRepository       = cashflowRepository;
            _logger                   = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<CustomerOrder>> GetAllCustomerOrdersAsync() =>
            await _repository.GetAllAsync();

        public async Task<CustomerOrder?> GetCustomerOrderByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        public async Task<CustomerOrder> CreateCustomerOrderWithTransactionAsync(CustomerOrder customerOrder)
        {
            // Begin UoW-managed transaction
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Validate customer
                var customer = await _customerRepository.GetByIdAsync(customerOrder.CustomerID);
                if (customer == null)
                    throw new InvalidOperationException($"Customer {customerOrder.CustomerID} not found.");

                // Extract item IDs from incoming order
                var itemIds = customerOrder.CustomerOrderElectronicItems
                                           .Select(i => i.E_ItemID)
                                           .ToList();

                // Check duplicates
                if (itemIds.Count != itemIds.Distinct().Count())
                    throw new InvalidOperationException("Order contains duplicate electronic items.");

                // Load all electronic items in ONE query
                var itemDict = await LoadElectronicItemsAsync(itemIds);

                decimal totalAmount = 0;

                // Loop through order line items
                foreach (var orderItem in customerOrder.CustomerOrderElectronicItems)
                {
                    if (!itemDict.TryGetValue(orderItem.E_ItemID, out var electronicItem))
                        throw new InvalidOperationException($"Electronic item {orderItem.E_ItemID} not found.");

                    if (orderItem.Quantity <= 0)
                        throw new InvalidOperationException($"Invalid quantity for item {orderItem.E_ItemID}.");

                    if (electronicItem.QOH < orderItem.Quantity)
                        throw new InvalidOperationException(
                            $"Insufficient stock for {electronicItem.ElectronicItemName}");

                    // Pricing
                    orderItem.UnitPrice = electronicItem.Price;
                    orderItem.SubTotal = orderItem.Quantity * electronicItem.Price;
                    totalAmount += orderItem.SubTotal;

                    // Deduct stock in memory
                    electronicItem.QOH -= orderItem.Quantity;
                }

                // Update order metadata
                customerOrder.TotalAmount = totalAmount;
                customerOrder.OrderDate = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);
                customerOrder.OrderStatus = OrderStatusEnum.Pending;
                customerOrder.OrderPaymentStatus = OrderPaymentStatusEnum.Partially_Paid;

                // Add order — EFCore will detect all changes (order + stock)
                await _repository.AddAsync(customerOrder);

                // Save everything together
                await _unitOfWork.CommitAsync();

                _logger.LogInformation(
                    "Customer order created: Id={Id}, TotalAmount={Total}",
                    customerOrder.OrderID, totalAmount);

                return customerOrder;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to create customer order.");
                throw;
            }
        }

        // Helper method : Loads multiple ElectronicItem entities in one DB call and returns them as a Dictionary.
        private async Task<Dictionary<int, ElectronicItem>> LoadElectronicItemsAsync(IEnumerable<int> ids)
        {
            var items = await _electronicItemRepository.GetAllByIdsAsync(ids.ToList());
            return items.ToDictionary(x => x.ElectronicItemID);
        }

        public async Task<CustomerOrder?> ModifyCustomerOrderStatusWithTransactionAsync(CustomerOrderStatusChangeRequestDto request)
        {
            // Current time
            var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            // Begin UoW-managed transaction
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Fetch order with all related entities in one query
                var order = await _repository.GetByIdWithAllRelatedAsync(request.OrderID);
                if (order == null)
                    throw new Exception("Customer order not found");

                var oldStatus = order.OrderStatus;

                // No change
                if (oldStatus == request.NewOrderStatus)
                    return order;

                // Validation: allowed transitions
                switch (oldStatus)
                {
                    case OrderStatusEnum.Pending:
                        if (request.NewOrderStatus != OrderStatusEnum.Shipped &&
                            request.NewOrderStatus != OrderStatusEnum.Cancelled)
                            throw new InvalidOperationException("Pending orders can only move to 'Shipped' or 'Cancelled'.");
                        break;

                    case OrderStatusEnum.Shipped:
                        if (request.NewOrderStatus != OrderStatusEnum.Delivered &&
                            request.NewOrderStatus != OrderStatusEnum.Cancelled)
                            throw new InvalidOperationException("Shipped orders can only move to 'Delivered' or 'Cancelled'.");
                        break;

                    case OrderStatusEnum.Delivered:
                        if (request.NewOrderStatus == OrderStatusEnum.Cancelled)
                        {
                            var deliveredDate = order.DeliveredDate ?? now;
                            if ((now - deliveredDate).TotalDays > BnplSystemConstants.FreeTrialPeriodDays)
                                throw new InvalidOperationException("Cannot cancel delivered orders after free trial period.");
                        }
                        else
                        {
                            throw new InvalidOperationException("Delivered orders cannot change status except cancellation within 14 days.");
                        }
                        break;

                    case OrderStatusEnum.Cancelled:
                        throw new InvalidOperationException("Cancelled orders cannot change status.");
                }

                // Perform status-specific actions
                switch (request.NewOrderStatus)
                {
                    case OrderStatusEnum.Cancelled:
                        CancelOrder(order, now); // Helper method (restock, cancel BNPL, cashflow)
                        break;

                    case OrderStatusEnum.Shipped:
                        order.ShippedDate = now;
                        break;

                    case OrderStatusEnum.Delivered:
                        order.DeliveredDate = now;
                        break;
                }

                // Update order status
                order.OrderStatus = request.NewOrderStatus;
                await _repository.UpdateAsync(order.OrderID, order);

                // No need to call UpdateAsync per entity; EF Core tracks all changes
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Customer order status updated: Id={Id}, Status={Status}", order.OrderID, order.OrderStatus);

                return order;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to update order status.");
                throw;
            }
        }

        // Helper method : CancelOrder
        private void CancelOrder(CustomerOrder order, DateTime now)
        {
            // Restock electronic items
            foreach (var item in order.CustomerOrderElectronicItems)
            {
                item.ElectronicItem.QOH += item.Quantity;
            }

            order.CancelledDate = now;

            // Refund cashflows
            foreach (var cashflow in order.Cashflows)
            {
                cashflow.CashflowStatus = CashflowStatusEnum.Refunded;
            }

            // Cancel BNPL plan and related entities
            if (order.BNPL_PLAN != null)
            {
                var plan = order.BNPL_PLAN;
                plan.Bnpl_Status = BnplStatusEnum.Cancelled;
                plan.CancelledAt = now;

                foreach (var installment in plan.BNPL_Installments)
                    installment.Bnpl_Installment_Status = BNPL_Installment_StatusEnum.Refunded;

                foreach (var snapshot in plan.BNPL_PlanSettlementSummaries)
                    snapshot.Bnpl_PlanSettlementSummary_Status = BNPL_PlanSettlementSummary_StatusEnum.Cancelled;
            }
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<CustomerOrder>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? paymentStatusId = null, int? orderStatusId = null, string? searchKey = null)
        {
            return await _repository.GetAllWithPaginationAsync(pageNumber, pageSize, paymentStatusId, orderStatusId, searchKey);
        }

        public async Task<PaginationResultDto<CustomerOrder>> GetAllByCustomerWithPaginationAsync(int customerId, int pageNumber, int pageSize, int? orderStatusId = null, string? searchKey = null)
        {
            return await _repository.GetAllByCustomerWithPaginationAsync(customerId, pageNumber, pageSize, orderStatusId, searchKey);
        }

        // Builds the object without DB Access
        public async Task<CustomerOrder> BuildCustomerOrderPaymentStatusUpdateRequestAsync(CustomerOrderPaymentStatusChangeRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var order = await _repository.GetByIdAsync(request.OrderID);

            if (order == null)
                throw new InvalidOperationException("Customer order not found");

            var oldStatus = order.OrderPaymentStatus;

            // No change – just return existing
            if (oldStatus == request.NewPaymentStatus)
                return order;

            // Validate allowed transitions
            ValidatePaymentStatusTransition(oldStatus, request.NewPaymentStatus);

            // Total paid calculation
            var totalPaid = await _cashflowRepository.SumCashflowsByOrderAsync(request.OrderID);

            var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            // Auto-complete to Fully Paid if payment is enough
            if (totalPaid >= order.TotalAmount)
            {
                order.OrderPaymentStatus = OrderPaymentStatusEnum.Fully_Paid;
                order.PaymentCompletedDate = now;
            }
            else
            {
                order.OrderPaymentStatus = request.NewPaymentStatus;
            }

            return order;
        }

        //Helper method : to validate payment update request 
        private void ValidatePaymentStatusTransition(OrderPaymentStatusEnum oldStatus, OrderPaymentStatusEnum newStatus)
        {
            switch (oldStatus)
            {
                case OrderPaymentStatusEnum.Partially_Paid:
                    if (newStatus != OrderPaymentStatusEnum.Fully_Paid &&
                        newStatus != OrderPaymentStatusEnum.Overdue)
                        throw new InvalidOperationException(
                            "Partially paid orders can only move to 'Fully_Paid' or 'Overdue'.");
                    break;

                case OrderPaymentStatusEnum.Fully_Paid:
                    if (newStatus != OrderPaymentStatusEnum.Refunded)
                        throw new InvalidOperationException(
                            "Fully paid orders can only move to 'Refunded'.");
                    break;

                case OrderPaymentStatusEnum.Overdue:
                    if (newStatus != OrderPaymentStatusEnum.Partially_Paid &&
                        newStatus != OrderPaymentStatusEnum.Fully_Paid)
                        throw new InvalidOperationException(
                            "Overdue orders can only move to 'Partially_Paid' or 'Fully_Paid'.");
                    break;

                case OrderPaymentStatusEnum.Refunded:
                    throw new InvalidOperationException(
                        "Refunded orders cannot change payment status.");
            }
        }
    }
}

