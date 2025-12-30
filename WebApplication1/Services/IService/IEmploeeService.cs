using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Services.IService
{
    public interface IEmployeeService
    {
        //CRUD operations
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
        Task<Employee?> GetEmployeeByIdAsync(int id);

        //Multiple Repository Operations (transactional)
        Task<Employee> CreateEmployeeWithTransactionAsync(Employee employee);

        //Single Repository Operations (save immediately)
        Task<Employee> UpdateEmployeeWithSaveAsync(int id, Employee employee);
        Task<Employee> UpdateEmployeeProfileWithSaveAsync(int id, Employee employee);

        //Custom Query Operations
        Task<PaginationResultDto<Employee>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? positionId, string? searchKey = null);
        Task<Employee?> GetEmployeeByUserIdAsync(int userId);
    }
}