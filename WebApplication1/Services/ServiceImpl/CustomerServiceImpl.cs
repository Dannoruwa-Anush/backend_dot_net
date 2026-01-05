using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.Services.IService.Audit;
using WebApplication1.UOW.IUOW;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.ServiceImpl
{
    public class CustomerServiceImpl : ICustomerService
    {
        private readonly ICustomerRepository _repository;
        private readonly IAppUnitOfWork _unitOfWork;

        //logger: for auditing
        // Audit Logging
        private readonly IAuditLogService _auditLogService;

        // Service-Level (Technical) Logging
        private readonly ILogger<CustomerServiceImpl> _logger;

        // Constructor
        public CustomerServiceImpl(ICustomerRepository repository, IAppUnitOfWork unitOfWork, IAuditLogService auditLogService, ILogger<CustomerServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<Customer>> GetAllCustomersAsync() =>
            await _repository.GetAllWithUserDeailsAsync();


        public async Task<Customer?> GetCustomerByIdAsync(int id) =>
            await _repository.GetWithUserDetailsByIdAsync(id);

        public async Task<Customer> CreateCustomerWithTransactionAsync(Customer customer)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Trim
                customer.User.Email = customer.User.Email.Trim().ToLower();
                customer.User.Password = customer.User.Password.Trim();

                var duplicate = await _repository.ExistsByPhoneNoAsync(customer.PhoneNo);
                if (duplicate)
                    throw new Exception($"Customer with phoneNo '{customer.PhoneNo}' already exists.");

                // Hash only if not already hashed
                if (!customer.User.Password.StartsWith("$2a$") && !customer.User.Password.StartsWith("$2b$"))
                {
                    customer.User.Password = BCrypt.Net.BCrypt.HashPassword(customer.User.Password);
                }

                await _repository.AddAsync(customer);
                await _unitOfWork.CommitAsync();

                _auditLogService.LogEntityAction(AuditActionTypeEnum.Create, "Customer", customer.CustomerID, customer.PhoneNo);
                return customer;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to create customer.");
                throw;
            }
        }

        public async Task<Customer> UpdateCustomerProfileWithSaveAsync(int id, Customer customer)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("Customer not found");

            var duplicate = await _repository.ExistsByPhoneNoAsync(customer.PhoneNo, id);
            if (duplicate)
                throw new Exception($"Customer with phoneNo '{customer.PhoneNo}' already exists.");

            var updatedCustomer = await _repository.UpdateProfileAsync(id, customer);
            await _unitOfWork.SaveChangesAsync();

            if (updatedCustomer == null)
                throw new Exception("Customer profile update failed.");

            _auditLogService.LogEntityAction(AuditActionTypeEnum.Update, "Customer", updatedCustomer.CustomerID, updatedCustomer.PhoneNo);
            return updatedCustomer;
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<Customer>> GetAllWithPaginationAsync(int pageNumber, int pageSize, string? searchKey = null)
        {
            return await _repository.GetAllWithPaginationAsync(pageNumber, pageSize, searchKey);
        }

        public async Task<Customer?> GetCustomerByUserIdAsync(int userId) =>
            await _repository.GetByUserIdAsync(userId);
    }
}