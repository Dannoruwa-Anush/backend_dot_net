using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.UOW.IUOW;

namespace WebApplication1.Services.ServiceImpl
{
    public class EmployeeServiceImpl : IEmployeeService
    {
        private readonly IEmployeeRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly IAppUnitOfWork _unitOfWork;

        //logger: for auditing
        // Service-Level (Technical) Logging
        private readonly ILogger<EmployeeServiceImpl> _logger;

        // Constructor
        public EmployeeServiceImpl(IEmployeeRepository repository, IUserRepository userRepository, IAppUnitOfWork unitOfWork, ILogger<EmployeeServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync() =>
            await _repository.GetAllWithUserDetailsAsync();

        public async Task<Employee?> GetEmployeeByIdAsync(int id) =>
            await _repository.GetWithUserDetailsByIdAsync(id);

        public async Task<Employee> CreateEmployeeWithTransactionAsync(Employee employee)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Trim
                employee.User.Email = employee.User.Email.Trim().ToLower();
                employee.User.Password = employee.User.Password.Trim();

                if (await _userRepository.EmailExistsAsync(employee.User.Email))
                    throw new Exception($"User with email '{employee.User.Email}' already exists.");

                // Hash only if not already hashed
                if (!employee.User.Password.StartsWith("$2a$") && !employee.User.Password.StartsWith("$2b$"))
                {
                    employee.User.Password = BCrypt.Net.BCrypt.HashPassword(employee.User.Password);
                }

                await _repository.AddAsync(employee);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Employee created: Id={Id}, PhoneNo={PhoneNo}", employee.EmployeeID, employee.EmployeeName);
                return employee;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to create employee.");
                throw;
            }
        }

        public async Task<Employee> UpdateEmployeeWithSaveAsync(int id, Employee employee)
        {
            var existingEmployee = await _repository.GetByIdAsync(id);
            if (existingEmployee == null)
                throw new Exception("Employee not found");

            var updatedEmployee = await _repository.UpdateAsync(id, employee);
            await _unitOfWork.SaveChangesAsync();

            if (updatedEmployee == null)
                throw new Exception("Employee update failed.");

            _logger.LogInformation("Employee updated: Id={Id}, PhoneNo={PhoneNo}", updatedEmployee.EmployeeID, updatedEmployee.EmployeeName);
            return updatedEmployee;
        }

        public async Task<Employee> UpdateEmployeeProfileWithSaveAsync(int id, Employee employee)
        {
            var existingEmployee = await _repository.GetByIdAsync(id);
            if (existingEmployee == null)
                throw new Exception("Employee not found");

            var updatedEmployee = await _repository.UpdateProfileAsync(id, employee);
            await _unitOfWork.SaveChangesAsync();

            if (updatedEmployee == null)
                throw new Exception("Employee profile update failed.");
                
            _logger.LogInformation("Employee profile updated: Id={Id}, EmployeeName={Name}", updatedEmployee.EmployeeID, updatedEmployee.EmployeeName);
            return updatedEmployee;
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<Employee>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? positionId, string? searchKey = null)
        {
            return await _repository.GetAllWithPaginationAsync(pageNumber, pageSize, positionId, searchKey);
        }

        public async Task<Employee?> GetEmployeeByUserIdAsync(int userId) =>
            await _repository.GetByUserIdAsync(userId);
    }
}