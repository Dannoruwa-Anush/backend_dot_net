using WebApplication1.DTOs.RequestDto.StatusChange;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Services.IService
{
    public interface ICustomerOrderService
    {
        //CRUD operations
        Task<IEnumerable<CustomerOrder>> GetAllCustomerOrdersAsync();
        Task<CustomerOrder?> GetCustomerOrderByIdAsync(int id);
        Task<CustomerOrder> AddCustomerOrderAsync(CustomerOrder customerOrder);
        Task<CustomerOrder?> UpdateCustomerOrderStatusAsync(CustomerOrderStatusChangeRequestDto requet);

        //Custom Query Operations
        Task<PaginationResultDto<CustomerOrder>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? paymentStatusId = null, int? orderStatusId = null, string? searchKey = null);
        Task<PaginationResultDto<CustomerOrder>> GetAllByCustomerWithPaginationAsync(int customerId, int pageNumber, int pageSize, int? orderStatusId = null, string? searchKey = null);       

        // Builds the object without DB Access
        Task<CustomerOrder> BuildCustomerOrderPaymentStatusUpdateRequestAsync(CustomerOrderPaymentStatusChangeRequestDto request);
    }
}