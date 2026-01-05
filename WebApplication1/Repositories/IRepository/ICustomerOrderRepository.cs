using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Repositories.IRepository
{
    public interface ICustomerOrderRepository
    {
        //CRUD operations
        Task<IEnumerable<CustomerOrder>> GetAllAsync();
        Task<IEnumerable<CustomerOrder>> GetAllWithCustomerDetailsAsync();
        Task<CustomerOrder?> GetByIdAsync(int id);
        Task<CustomerOrder?> GetWithCustomerOrderDetailsByIdAsync(int id);
        Task<CustomerOrder?> GetWithFinancialDetailsByIdAsync(int id);
        Task AddAsync(CustomerOrder customerOrder);
        Task<CustomerOrder?> UpdateAsync(int id, CustomerOrder customerOrder);

        //Custom Query Operations 
        Task<PaginationResultDto<CustomerOrder>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? paymentStatusId = null, int? orderStatusId = null, string? searchKey = null);
        Task<bool> ExistsByCustomerAsync(int customerId);
        Task<bool> ExistsPendingOrderForCustomerAsync(int customerId);
        Task<PaginationResultDto<CustomerOrder>> GetAllByCustomerWithPaginationAsync(int customerId, int pageNumber, int pageSize, int? orderStatusId = null, string? searchKey = null);       
        Task<IEnumerable<CustomerOrder>> GetAllPaymentPendingByPhysicalShopSessionIdAsync(int PhysicalShopSessionId);
    }
}