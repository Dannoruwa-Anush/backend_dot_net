using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Repositories.IRepository
{
    public interface ICustomerRepository
    {
        //CRUD operations
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<IEnumerable<Customer>> GetAllWithUserDeailsAsync();
        Task<Customer?> GetByIdAsync(int id);
        Task<Customer?> GetWithUserDetailsByIdAsync(int id);
        Task AddAsync(Customer customer);
        Task<Customer?> UpdateProfileAsync(int id, Customer customer);
        Task<bool> DeleteAsync(int id);

        //Custom Query Operations
        Task<PaginationResultDto<Customer>> GetAllWithPaginationAsync(int pageNumber, int pageSize, string? searchKey = null);
        Task<bool> ExistsByPhoneNoAsync(string phoneNo);
        Task<bool> ExistsByPhoneNoAsync(string phoneNo, int excludeId);
        Task<Customer?> GetByUserIdAsync(int userId);
    }
}