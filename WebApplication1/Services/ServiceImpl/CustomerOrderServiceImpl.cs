using WebApplication1.DTOs.RequestDto.Custom;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.ServiceImpl
{
    public class CustomerOrderServiceImpl : ICustomerOrderService
    {
        private readonly ICustomerOrderRepository _repository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IElectronicItemRepository _electronicItemRepository;
        private readonly ICustomerOrderElectronicItemRepository _customerOrderElectronicItemRepository;

        //logger: for auditing
        private readonly ILogger<CustomerOrderServiceImpl> _logger;

        // Constructor
        public CustomerOrderServiceImpl(ICustomerOrderRepository repository, ICustomerRepository customerRepository, IElectronicItemRepository electronicItemRepository, ICustomerOrderElectronicItemRepository customerOrderElectronicItemRepository, ILogger<CustomerOrderServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _customerRepository = customerRepository;
            _electronicItemRepository = electronicItemRepository;
            _customerOrderElectronicItemRepository = customerOrderElectronicItemRepository;
            _logger = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<CustomerOrder>> GetAllCustomerOrdersAsync() =>
            await _repository.GetAllAsync();

        public async Task<CustomerOrder?> GetCustomerOrderByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        public async Task<CustomerOrder> AddCustomerOrderAsync(CustomerOrder customerOrder)
        {
            // Begin EF Core transaction
            await using var transaction = await _repository.BeginTransactionAsync();
            try
            {
                // Validate customer exists
                var customer = await _customerRepository.GetByIdAsync(customerOrder.CustomerID);
                if (customer == null)
                    throw new InvalidOperationException($"Customer {customerOrder.CustomerID} not found.");

                decimal totalAmount = 0;

                // Validate stock & calculate subtotal for each order item
                // Make sure there are no duplicate E_ItemID in the order
                var itemIds = new HashSet<int>();
                foreach (var orderItem in customerOrder.CustomerOrderElectronicItems)
                {
                    if (!itemIds.Add(orderItem.E_ItemID))
                        throw new InvalidOperationException($"Duplicate item in order: {orderItem.E_ItemID}");

                    var electronicItem = await _electronicItemRepository.GetByIdAsync(orderItem.E_ItemID);
                    if (electronicItem == null)
                        throw new InvalidOperationException($"Electronic item {orderItem.E_ItemID} not found.");

                    if (electronicItem.QOH < orderItem.Quantity)
                        throw new InvalidOperationException($"Insufficient stock for {electronicItem.ElectronicItemName}");

                    // Compute subtotal
                    orderItem.UnitPrice = electronicItem.Price;
                    orderItem.SubTotal = orderItem.Quantity * electronicItem.Price;
                    totalAmount += orderItem.SubTotal;

                    // Deduct stock
                    electronicItem.QOH -= orderItem.Quantity;
                    await _electronicItemRepository.UpdateAsync(electronicItem.ElectronicItemID, electronicItem);
                }

                // Set order total & default statuses
                customerOrder.TotalAmount = totalAmount;
                customerOrder.OrderStatus = OrderStatusEnum.Pending;
                customerOrder.OrderPaymentStatus = OrderPaymentStatusEnum.Partially_Paid;

                // Save order along with its child items in one go
                // EF Core will automatically insert CustomerOrderElectronicItems via navigation property
                await _repository.AddAsync(customerOrder);
                await _repository.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();

                _logger.LogInformation("Customer order created: Id={Id}, TotalAmount={Total}", customerOrder.OrderID, totalAmount);

                return customerOrder;
            }
            catch (Exception ex)
            {
                // Rollback transaction if anything fails
                await transaction.RollbackAsync();

                _logger.LogError(ex, "Failed to create customer order.");
                throw;
            }
        }

        public async Task<CustomerOrder?> UpdateCustomerOrderAsync(int id, CustomerOrderUpdateDto updateDto)
        {
            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));

            if (updateDto.OrderStatus.HasValue)
            {
                return await UpdateCustomerOrderStatusAsync(id, updateDto.OrderStatus.Value);
            }
            else if (updateDto.PaymentStatus.HasValue)
            {
                return await UpdateCustomerOrderPaymentStatusAsync(id, updateDto.PaymentStatus.Value);
            }
            else
            {
                throw new Exception("No valid update fields provided");
            }
        }

        //Helper method : to Update PaymentStatus
        private async Task<CustomerOrder?> UpdateCustomerOrderPaymentStatusAsync(int id, OrderPaymentStatusEnum newOrderPaymentStatus)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("Customer order not found");

            var oldPayment = existing.OrderPaymentStatus;

            if (oldPayment == newOrderPaymentStatus)
                return existing; // No change

            switch (oldPayment)
            {
                case OrderPaymentStatusEnum.Partially_Paid:
                    if (newOrderPaymentStatus != OrderPaymentStatusEnum.Fully_Paid &&
                        newOrderPaymentStatus != OrderPaymentStatusEnum.Overdue)
                        throw new InvalidOperationException("Partially paid orders can only move to 'Fully_Paid' or 'Overdue'.");
                    break;

                case OrderPaymentStatusEnum.Fully_Paid:
                    if (newOrderPaymentStatus != OrderPaymentStatusEnum.Refunded)
                        throw new InvalidOperationException("Fully paid orders can only move to 'Refunded'.");
                    break;

                case OrderPaymentStatusEnum.Overdue:
                    if (newOrderPaymentStatus != OrderPaymentStatusEnum.Fully_Paid)
                        throw new InvalidOperationException("Overdue orders can only move to 'Fully_Paid'.");
                    break;

                case OrderPaymentStatusEnum.Refunded:
                    throw new InvalidOperationException("Refunded orders cannot change payment status.");
            }

            existing.OrderPaymentStatus = newOrderPaymentStatus;

            await _repository.UpdateAsync(id, existing);
            _logger.LogInformation("Customer payment status updated: Id={Id}, PaymentStatus={PaymentStatus}", existing.OrderID, existing.OrderPaymentStatus);

            return existing;
        }

        //Helper method : to update OrderStatus
        private async Task<CustomerOrder?> UpdateCustomerOrderStatusAsync(int id, OrderStatusEnum newOrderStatus)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("Customer order not found");

            var oldStatus = existing.OrderStatus;

            if (oldStatus == newOrderStatus)
                return existing; // No change

            // Validation: allowed transitions
            switch (oldStatus)
            {
                case OrderStatusEnum.Pending:
                    if (newOrderStatus != OrderStatusEnum.Shipped && newOrderStatus != OrderStatusEnum.Cancelled)
                        throw new InvalidOperationException("Pending orders can only move to 'Shipped' or 'Cancelled'.");
                    break;

                case OrderStatusEnum.Shipped:
                    if (newOrderStatus != OrderStatusEnum.Delivered && newOrderStatus != OrderStatusEnum.Cancelled)
                        throw new InvalidOperationException("Shipped orders can only move to 'Delivered' or 'Cancelled'.");
                    break;

                case OrderStatusEnum.Delivered:
                    if (newOrderStatus == OrderStatusEnum.Cancelled)
                    {
                        // 14-day cancellation window
                        var daysSinceDelivery = (DateTime.UtcNow - (existing.DeliveredDate ?? DateTime.UtcNow)).TotalDays;
                        if (daysSinceDelivery > 14)
                            throw new InvalidOperationException("Cannot cancel delivered orders after 14 days.");
                    }
                    else
                    {
                        throw new InvalidOperationException("Delivered orders cannot change status except cancellation within 14 days.");
                    }
                    break;

                case OrderStatusEnum.Cancelled:
                    throw new InvalidOperationException("Cancelled orders cannot change status.");
            }

            // Begin transaction for atomicity
            await using var transaction = await _repository.BeginTransactionAsync();
            try
            {
                // If cancelling, reverse stock
                if (newOrderStatus == OrderStatusEnum.Cancelled)
                {
                    foreach (var item in existing.CustomerOrderElectronicItems)
                    {
                        var electronicItem = await _electronicItemRepository.GetByIdAsync(item.E_ItemID);
                        if (electronicItem != null)
                        {
                            electronicItem.QOH += item.Quantity; // Restock
                            await _electronicItemRepository.UpdateAsync(electronicItem.ElectronicItemID, electronicItem);
                        }
                    }

                    existing.CancelledDate = DateTime.UtcNow;
                }
                else if (newOrderStatus == OrderStatusEnum.Shipped)
                {
                    existing.ShippedDate = DateTime.UtcNow;
                }
                else if (newOrderStatus == OrderStatusEnum.Delivered)
                {
                    existing.DeliveredDate = DateTime.UtcNow;
                }

                existing.OrderStatus = newOrderStatus;

                await _repository.UpdateAsync(id, existing);
                await _repository.SaveChangesAsync();

                await transaction.CommitAsync();
                _logger.LogInformation("Customer order status updated: Id={Id}, Status={Status}", existing.OrderID, existing.OrderStatus);

                return existing;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to update order status.");
                throw;
            }
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<CustomerOrder>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? paymentStatusId = null, int? orderStatusId = null, string? searchKey = null)
        {
            return await _repository.GetAllWithPaginationAsync(pageNumber, pageSize, paymentStatusId, orderStatusId, searchKey);
        }
    }
}

