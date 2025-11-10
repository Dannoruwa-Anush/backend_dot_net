using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;

namespace WebApplication1.Services.ServiceImpl
{
    public class EmployeeServiceImpl : IEmployeeService
    {
        private readonly IEmployeeRepository _repository;

        //logger: for auditing
        private readonly ILogger<EmployeeServiceImpl> _logger;

        // Constructor
        public EmployeeServiceImpl(IEmployeeRepository repository, ILogger<EmployeeServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _logger = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync() =>
            await _repository.GetAllAsync();

        public async Task<Employee?> GetEmployeeByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);
        
        public async Task<Employee> AddEmployeeAsync(Employee employee)
        {
            await _repository.AddAsync(employee);

            _logger.LogInformation("Employee created: Id={Id}, EmploeeName={Name}", employee.EmployeeID, employee.EmployeeName);
            return employee;
        }

        public async Task<Employee> UpdateEmployeeAsync(int id, Employee employee)
        {
            var existingEmployee = _repository.GetByIdAsync(id);
            if (existingEmployee == null)
                throw new Exception("Employee not found");

            var updatedEmployee = await _repository.UpdateAsync(id, employee);

            if (updatedEmployee != null)
            {
                _logger.LogInformation("Employee updated: Id={Id}, EmployeeName={Name}", updatedEmployee.EmployeeID, updatedEmployee.EmployeeName);
                return updatedEmployee;
            }
            
            throw new Exception("Employee update failed.");
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<Employee>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? positionId, string? searchKey = null)
        {
            return await _repository.GetAllWithPaginationAsync(pageNumber, pageSize, positionId, searchKey);
        }
    }
}