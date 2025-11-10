using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Services.IService
{
    public interface IEmployeeService
    {
        //CRUD operations
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
        Task<Employee?> GetEmployeeByIdAsync(int id);
        Task<Employee> AddEmployeeAsync(Employee employee);
        Task<Employee> UpdateEmployeeAsync(int id, Employee employee);

        //Custom Query Operations
        Task<PaginationResultDto<Employee>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? positionId, string? searchKey = null);
    }
}