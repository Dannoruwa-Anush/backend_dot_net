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
            _repository = repository;
            _unitOfWork = unitOfWork;
            _customerRepository = customerRepository;
            _electronicItemRepository = electronicItemRepository;
            _cashflowRepository = cashflowRepository;
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
                var customer = await _customerRepository.GetByIdAsync(customerOrder.CustomerID)
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
                customerOrder.OrderPaymentStatus = OrderPaymentStatusEnum.Partially_Paid;

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
            var items = await _electronicItemRepository.GetAllByIdsAsync(ids.ToList());
            return items.ToDictionary(x => x.ElectronicItemID);
        }

        // Update : Order Status
        public async Task<CustomerOrder?> ModifyCustomerOrderStatusWithTransactionAsync(CustomerOrderStatusChangeRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var order = await _repository.GetWithFinancialDetailsByIdAsync(request.OrderID)
                    ?? throw new InvalidOperationException("Customer order not found");

                if (order.OrderStatus == request.NewOrderStatus)
                    return order;

                ValidateOrderStatusTransition(order, request.NewOrderStatus, now);
                ApplyOrderStatusChanges(order, request.NewOrderStatus, now);

                await _repository.UpdateAsync(order.OrderID, order);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Customer order status updated: Id={Id}, Status={Status}", order.OrderID, order.OrderStatus);
                return order;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to update customer order status for OrderID={OrderID}", request.OrderID);
                throw;
            }
        }

        public async Task<CustomerOrder?> BuildCustomerOrderPaymentStatusUpdateRequestAsync(CustomerOrderPaymentStatusChangeRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var existingOrder = await _repository.GetByIdAsync(request.OrderID)
                ?? throw new InvalidOperationException("Customer order not found");

            if (existingOrder.OrderPaymentStatus == request.NewPaymentStatus)
                return existingOrder;

            ValidatePaymentStatusTransition(existingOrder.OrderPaymentStatus, request.NewPaymentStatus);

            var totalPaid = await _cashflowRepository.SumCashflowsByOrderAsync(request.OrderID);
            var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            existingOrder.OrderPaymentStatus = totalPaid >= existingOrder.TotalAmount
                ? OrderPaymentStatusEnum.Fully_Paid
                : request.NewPaymentStatus;

            if (existingOrder.OrderPaymentStatus == OrderPaymentStatusEnum.Fully_Paid)
                existingOrder.PaymentCompletedDate = now;

            return await _repository.UpdateAsync(existingOrder.OrderID, existingOrder);
        }

        //Helper Method : ValidateOrderStatusTransition
        private void ValidateOrderStatusTransition(CustomerOrder order, OrderStatusEnum newStatus, DateTime now)
        {
            switch (order.OrderStatus)
            {
                case OrderStatusEnum.Pending:
                case OrderStatusEnum.Shipped:
                    if (newStatus != OrderStatusEnum.Shipped &&
                        newStatus != OrderStatusEnum.Delivered &&
                        newStatus != OrderStatusEnum.Cancel_Pending)
                    {
                        throw new InvalidOperationException(
                            $"{order.OrderStatus} orders can only move to Shipped, Delivered, or Cancel_Pending.");
                    }
                    break;

                case OrderStatusEnum.Delivered:
                    if (newStatus == OrderStatusEnum.Cancel_Pending)
                    {
                        var deliveredDate = order.DeliveredDate ?? now;
                        if ((now - deliveredDate).TotalDays > BnplSystemConstants.FreeTrialPeriodDays)
                            throw new InvalidOperationException("Cannot request cancellation for delivered orders after free trial period.");
                    }
                    else
                    {
                        throw new InvalidOperationException("Delivered orders cannot change status except Cancel_Pending within free trial period.");
                    }
                    break;

                case OrderStatusEnum.Cancel_Pending:
                    if (newStatus != OrderStatusEnum.Cancelled)
                        throw new InvalidOperationException("Cancel_Pending orders can only move to Cancelled.");
                    break;

                case OrderStatusEnum.Cancelled:
                    throw new InvalidOperationException("Cancelled orders cannot change status.");
            }
        }

        //Helper Method : ApplyOrderStatusChanges
        // Helper Method: Applies status changes to the order
        private void ApplyOrderStatusChanges(CustomerOrder order, OrderStatusEnum newStatus, DateTime now)
        {
            switch (newStatus)
            {
                case OrderStatusEnum.Cancelled:
                    order.OrderStatus = OrderStatusEnum.Cancelled;
                    CancelOrder(order, now); // performs restock, refunds, cancel BNPL, sets CancelledDate
                    break;

                case OrderStatusEnum.Shipped:
                    order.OrderStatus = OrderStatusEnum.Shipped;
                    order.ShippedDate = now;
                    break;

                case OrderStatusEnum.Delivered:
                    order.OrderStatus = OrderStatusEnum.Delivered;
                    order.DeliveredDate = now;
                    break;
            }
        }

        //Helper Method : CancelOrder
        private void CancelOrder(CustomerOrder order, DateTime now)
        {
            order.CancelledDate = now;

            foreach (var item in order.CustomerOrderElectronicItems)
                item.ElectronicItem.QOH += item.Quantity;

            foreach (var cashflow in order.Cashflows)
                cashflow.CashflowStatus = CashflowStatusEnum.Refunded;

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

        //Custom Query Operations
        public async Task<PaginationResultDto<CustomerOrder>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? paymentStatusId = null, int? orderStatusId = null, string? searchKey = null) =>
            await _repository.GetAllWithPaginationAsync(pageNumber, pageSize, paymentStatusId, orderStatusId, searchKey);

        public async Task<PaginationResultDto<CustomerOrder>> GetAllByCustomerWithPaginationAsync(int customerId, int pageNumber, int pageSize, int? orderStatusId = null, string? searchKey = null) =>
            await _repository.GetAllByCustomerWithPaginationAsync(customerId, pageNumber, pageSize, orderStatusId, searchKey);
    }
}
