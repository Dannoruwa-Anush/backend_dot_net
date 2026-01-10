using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Services.IService
{
    public interface ICustomerService
    {
        //CRUD operations
        Task<IEnumerable<Customer>> GetAllCustomersAsync();
        Task<Customer?> GetCustomerByIdAsync(int id);

        //Multiple Repository Operations (transactional)
        Task<Customer> CreateCustomerWithTransactionAsync(Customer customer);

        //Single Repository Operations (save immediately)
        Task<Customer> UpdateCustomerProfileWithSaveAsync(int id, Customer customer);

        //Custom Query Operations
        Task<PaginationResultDto<Customer>> GetAllWithPaginationAsync(int pageNumber, int pageSize, string? searchKey = null);
        Task<Customer?> GetCustomerByUserIdAsync(int userId);
        Task<Customer?> GetCustomerByUserEmailAsync(string email);
    }
}