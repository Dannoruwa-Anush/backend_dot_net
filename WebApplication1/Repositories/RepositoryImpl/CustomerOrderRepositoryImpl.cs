using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Repositories.RepositoryImpl
{
    public class CustomerOrderRepositoryImpl : ICustomerOrderRepository
    {
        private readonly AppDbContext _context;

        // Constructor
        public CustomerOrderRepositoryImpl(AppDbContext context)
        {
            // Dependency injection
            _context = context;
        }

        //CRUD Operations
        public async Task<IEnumerable<CustomerOrder>> GetAllAsync() =>
            await _context.CustomerOrders.ToListAsync();

        public async Task<CustomerOrder?> GetByIdAsync(int id) =>
            await _context.CustomerOrders.FindAsync(id);

        public async Task AddAsync(CustomerOrder customerOrder)
        {
            await _context.CustomerOrders.AddAsync(customerOrder);
            await _context.SaveChangesAsync();
        }

        //Custom Query Operations
        public async Task<CustomerOrder?> UpdateOrderStatusAsync(int id, OrderStatusEnum newStatus)
        {
            var existing = await _context.CustomerOrders.FindAsync(id);
            if (existing == null)
                return null;

            var oldStatus = existing.OrderStatus;

            if (oldStatus == newStatus)
                return existing; // No change

            switch (oldStatus)
            {
                case OrderStatusEnum.Pending:
                    if (newStatus != OrderStatusEnum.Shipped && newStatus != OrderStatusEnum.Cancelled)
                        throw new InvalidOperationException("Pending orders can only move to 'Shipped' or 'Cancelled'.");
                    break;

                case OrderStatusEnum.Shipped:
                    if (newStatus != OrderStatusEnum.Delivered && newStatus != OrderStatusEnum.Cancelled)
                        throw new InvalidOperationException("Shipped orders can only move to 'Delivered' or 'Cancelled'.");
                    break;

                case OrderStatusEnum.Delivered:
                    if (newStatus != OrderStatusEnum.Cancelled)
                        throw new InvalidOperationException("Delivered orders can only move to 'Cancelled' within 14 days.");
                    break;

                case OrderStatusEnum.Cancelled:
                    throw new InvalidOperationException("Cancelled orders cannot change status.");
            }

            // Apply date tracking
            switch (newStatus)
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

            existing.OrderStatus = newStatus;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.CustomerOrders.Update(existing);
            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<CustomerOrder?> UpdatePaymentStatusAsync(int id, OrderPaymentStatusEnum newPaymentStatus)
        {
            var existing = await _context.CustomerOrders.FindAsync(id);
            if (existing == null)
                return null;

            var oldPayment = existing.PaymentStatus;

            if (oldPayment == newPaymentStatus)
                return existing; // No change

            switch (oldPayment)
            {
                case OrderPaymentStatusEnum.Partially_Paid:
                    if (newPaymentStatus != OrderPaymentStatusEnum.Fully_Paid &&
                        newPaymentStatus != OrderPaymentStatusEnum.Overdue)
                        throw new InvalidOperationException("Partially paid orders can only move to 'Fully_Paid' or 'Overdue'.");
                    break;

                case OrderPaymentStatusEnum.Fully_Paid:
                    if (newPaymentStatus != OrderPaymentStatusEnum.Refunded)
                        throw new InvalidOperationException("Fully paid orders can only move to 'Refunded'.");
                    break;

                case OrderPaymentStatusEnum.Overdue:
                    if (newPaymentStatus != OrderPaymentStatusEnum.Fully_Paid)
                        throw new InvalidOperationException("Overdue orders can only move to 'Fully_Paid'.");
                    break;

                case OrderPaymentStatusEnum.Refunded:
                    throw new InvalidOperationException("Refunded orders cannot change payment status.");
            }

            existing.PaymentStatus = newPaymentStatus;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.CustomerOrders.Update(existing);
            await _context.SaveChangesAsync();

            return existing;
        }
        
        public async Task<bool> ExistsByCustomerAsync(int customerId)
        {
            return await _context.CustomerOrders.AnyAsync(o => o.CustomerID == customerId);
        }
    }
}