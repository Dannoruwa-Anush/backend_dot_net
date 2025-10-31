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
            _repository                             = repository;
            _customerRepository                     = customerRepository;
            _electronicItemRepository               = electronicItemRepository;
            _customerOrderElectronicItemRepository  = customerOrderElectronicItemRepository;
            _logger                                 = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<CustomerOrder>> GetAllCustomerOrdersAsync() =>
            await _repository.GetAllAsync();

        public async Task<CustomerOrder?> GetCustomerOrderByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        public Task<CustomerOrder> AddCustomerOrderAsync(CustomerOrder customerOrder)
        {
            throw new NotImplementedException();
        }

        public async Task<CustomerOrder?> UpdateCustomerOrderPaymentStatusAsync(int id, OrderPaymentStatusEnum newOrderPaymentStatus)
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
            existing.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(id, existing);
            _logger.LogInformation("Customer payment status updated: Id={Id}, PaymentStatus={PaymentStatus}", existing.OrderID, existing.OrderPaymentStatus);

            return existing;
        }

        public async Task<CustomerOrder?> UpdateCustomerOrderStatusAsync(int id, OrderStatusEnum newOrderStatus)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("Customer order not found");

            var oldStatus = existing.OrderStatus;

            if (oldStatus == newOrderStatus)
                return existing; // No change

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
                    if (newOrderStatus != OrderStatusEnum.Cancelled)
                        throw new InvalidOperationException("Delivered orders can only move to 'Cancelled' within 14 days.");
                    break;

                case OrderStatusEnum.Cancelled:
                    throw new InvalidOperationException("Cancelled orders cannot change status.");
            }

            // Apply date tracking
            switch (newOrderStatus)
            {
                case OrderStatusEnum.Shipped:
                    existing.ShippingDate = DateTime.UtcNow;
                    break;
                case OrderStatusEnum.Delivered:
                    existing.DeliveredDate = DateTime.UtcNow;
                    break;
                case OrderStatusEnum.Cancelled:
                    existing.CancelledDate = DateTime.UtcNow;
                    break;
            }

            existing.OrderStatus = newOrderStatus;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(id, existing);
            _logger.LogInformation("Customer order status updated: Id={Id}, OrderStatus={OrderStatus}", existing.OrderID, existing.OrderStatus);
            return existing;
        }

        //Custom Query Operations
    }
}

