using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Repositories.IRepository
{
    public interface ICustomerOrderRepository
    {
        //CRUD operations
        Task<IEnumerable<CustomerOrder>> GetAllAsync();
        Task<CustomerOrder?> GetByIdAsync(int id);
        Task AddAsync(CustomerOrder customerOrder);

        //Custom Query Operations
        Task<CustomerOrder?> UpdateOrderStatusAsync(int id, OrderStatusEnum newStatus);
        Task<CustomerOrder?> UpdatePaymentStatusAsync(int id, OrderPaymentStatusEnum newPaymentStatus);

        
        //Task<PaginationResultDto<CustomerOrder>> GetAllWithPaginationAsync(int pageNumber, int pageSize);
        //Task<IEnumerable<CustomerOrder>> GetAllBySearchKeyAsync(string searchKey);
        //Task<IEnumerable<CustomerOrder>> GetAllByPaymentStatusAsync(string paymentStatus);
        //Task<IEnumerable<CustomerOrder>> GetAllByBnplPlansync(int bnplPlanId);
        Task<bool> ExistsByCustomerAsync(int customerId);
    }
}