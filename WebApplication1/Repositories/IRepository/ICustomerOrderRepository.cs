using Microsoft.EntityFrameworkCore.Storage;
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
        Task<CustomerOrder?> UpdateAsync(int id, CustomerOrder customerOrder);

        //Custom Query Operations        
        //Task<PaginationResultDto<CustomerOrder>> GetAllWithPaginationAsync(int pageNumber, int pageSize);
        //Task<IEnumerable<CustomerOrder>> GetAllBySearchKeyAsync(string searchKey);
        //Task<IEnumerable<CustomerOrder>> GetAllByPaymentStatusAsync(string paymentStatus);
        //Task<IEnumerable<CustomerOrder>> GetAllByBnplPlansync(int bnplPlanId);
        Task<bool> ExistsByCustomerAsync(int customerId);

        // EF transaction support
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task SaveChangesAsync();
    }
}