using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Repositories.IRepository
{
    public interface IEmployeeRepository
    {
         //CRUD operations
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<IEnumerable<Employee>> GetAllWithUserDetailsAsync();
        Task<Employee?> GetByIdAsync(int id);
        Task<Employee?> GetWithUserDetailsByIdAsync(int id);
        Task AddAsync(Employee employee);
        Task<Employee?> UpdateAsync(int id, Employee employee);
        Task<bool> DeleteAsync(int id);

        //Custom Query Operations
        Task<PaginationResultDto<Employee>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? positionId, string? searchKey = null);
        Task<Employee?> GetByUserIdAsync(int userId);
    }
}