using WebApplication1.DTOs.RequestDto.StatusChange;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.Services.IService.Helper;
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
        private readonly IOrderFinancialService  _orderFinancialService;

        //logger: for auditing
        private readonly ILogger<CustomerOrderServiceImpl> _logger;

        // Constructor
        public CustomerOrderServiceImpl(ICustomerOrderRepository repository, IAppUnitOfWork unitOfWork, ICustomerService customerService, IElectronicItemService electronicItemService, IOrderFinancialService orderFinancialService, ILogger<CustomerOrderServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _unitOfWork = unitOfWork;
            _customerService = customerService;
            _electronicItemService = electronicItemService;
            _orderFinancialService = orderFinancialService;
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

                ValidateOrderStatusTransition(order, request.NewOrderStatus, now);
                await ApplyOrderStatusChangesAsync(order, request.NewOrderStatus, now);

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
                            throw new InvalidOperationException(
                                "Cannot request cancellation for delivered orders after free trial period.");
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            "Delivered orders cannot change status except Cancel_Pending within free trial period.");
                    }
                    break;

                case OrderStatusEnum.Cancel_Pending:
                    if (newStatus != OrderStatusEnum.Cancelled &&
                        newStatus != OrderStatusEnum.DeliveredAfterCancellationRejected)
                    {
                        throw new InvalidOperationException(
                            "Cancel_Pending orders can only move to Cancelled (approved) or DeliveredAfterCancellationRejected (rejected).");
                    }
                    break;

                case OrderStatusEnum.Cancelled:
                case OrderStatusEnum.DeliveredAfterCancellationRejected:
                    throw new InvalidOperationException($"{order.OrderStatus} orders cannot change status.");
            }
        }

        // Helper Method: Applies status changes to the order
        private async Task ApplyOrderStatusChangesAsync(CustomerOrder order, OrderStatusEnum newStatus, DateTime now)
        {
            switch (newStatus)
            {
                case OrderStatusEnum.Cancel_Pending:
                    order.OrderStatus = OrderStatusEnum.Cancel_Pending;
                    order.CancellationRequestDate = now;
                    order.CancellationApproved = null; // pending
                    break;

                case OrderStatusEnum.Cancelled:
                    order.OrderStatus = OrderStatusEnum.Cancelled;
                    order.CancelledDate = now;
                    order.CancellationApproved = true;
                    await HandleCancelOrderAsync(order);
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
                    order.DeliveredDate = now;
                    order.CancellationApproved = false;
                    order.CancellationRejectionReason ??= "Cancellation rejected";
                    break;
            }
        }

        //Helper Method : CancelOrder
        private async Task HandleCancelOrderAsync(CustomerOrder order)
        {
            HandleRestock(order);

            //After cancellation Confirmed : Refund 
            await _orderFinancialService.BuildPaymentUpdateRequestAsync(order, OrderPaymentStatusEnum.Refunded);
        }

        //Helper Method : Restock
        private void HandleRestock(CustomerOrder order)
        {
            foreach (var item in order.CustomerOrderElectronicItems)
                item.ElectronicItem.QOH += item.Quantity;
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<CustomerOrder>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? paymentStatusId = null, int? orderStatusId = null, string? searchKey = null) =>
            await _repository.GetAllWithPaginationAsync(pageNumber, pageSize, paymentStatusId, orderStatusId, searchKey);

        public async Task<PaginationResultDto<CustomerOrder>> GetAllByCustomerWithPaginationAsync(int customerId, int pageNumber, int pageSize, int? orderStatusId = null, string? searchKey = null) =>
            await _repository.GetAllByCustomerWithPaginationAsync(customerId, pageNumber, pageSize, orderStatusId, searchKey);
    }
}
