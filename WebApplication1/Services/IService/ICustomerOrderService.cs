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

        //Multiple Repository Operations (transactional)
        Task<CustomerOrder> CreateCustomerOrderWithTransactionAsync(CustomerOrder customerOrder);
        Task<CustomerOrder?> ModifyCustomerOrderStatusWithTransactionAsync(int orderId, CustomerOrderStatusChangeRequestDto requet);

        //Custom Query Operations
        Task<CustomerOrder?> GetCustomerOrderWithFinancialDetailsByIdAsync(int id);
        Task<PaginationResultDto<CustomerOrder>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? paymentStatusId = null, int? orderStatusId = null, string? searchKey = null);
        Task<PaginationResultDto<CustomerOrder>> GetAllByCustomerWithPaginationAsync(int customerId, int pageNumber, int pageSize, int? orderStatusId = null, string? searchKey = null);       
    }
}