using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.IService
{
    public interface ICustomerOrderService
    {
        //CRUD operations
        Task<IEnumerable<CustomerOrder>> GetAllCustomerOrdersAsync();
        Task<CustomerOrder?> GetCustomerOrderByIdAsync(int id);
        Task<CustomerOrder> AddCustomerOrderAsync(CustomerOrder customerOrder);
        Task<CustomerOrder?> UpdateCustomerOrderStatusAsync(int id, OrderStatusEnum newOrderStatus);
        Task<CustomerOrder?> UpdateCustomerOrderPaymentStatusAsync(int id, OrderPaymentStatusEnum newOrderPaymentStatus);

        //Custom Query Operations
        Task<PaginationResultDto<CustomerOrder>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? paymentStatusId = null, int? orderStatusId = null, string? searchKey = null);
    }
}