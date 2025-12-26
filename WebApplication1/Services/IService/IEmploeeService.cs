using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Services.IService
{
    public interface IEmployeeService
    {
        //CRUD operations
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
        Task<Employee?> GetEmployeeByIdAsync(int id);

        //Single Repository Operations (save immediately)
        Task<Employee> AddEmployeeWithSaveAsync(Employee employee);
        Task<Employee> UpdateEmployeeWithSaveAsync(int id, Employee employee);

        //Custom Query Operations
        Task<PaginationResultDto<Employee>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? positionId, string? searchKey = null);
        Task<Employee?> GetEmployeeByUserIdAsync(int userId);
    }
}