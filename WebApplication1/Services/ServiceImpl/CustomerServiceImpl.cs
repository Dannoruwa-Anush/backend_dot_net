using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.UOW.IUOW;

namespace WebApplication1.Services.ServiceImpl
{
    public class CustomerServiceImpl : ICustomerService
    {
        private readonly ICustomerRepository _repository;
        private readonly IAppUnitOfWork _unitOfWork;

        //logger: for auditing
        private readonly ILogger<CustomerServiceImpl> _logger;

        // Constructor
        public CustomerServiceImpl(ICustomerRepository repository, IAppUnitOfWork unitOfWork, ILogger<CustomerServiceImpl> logger)
        {
            // Dependency injection
            _repository     = repository;
            _unitOfWork     = unitOfWork;
            _logger         = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<Customer>> GetAllCustomersAsync() =>
            await _repository.GetAllWithUserDeailsAsync();


        public async Task<Customer?> GetCustomerByIdAsync(int id) =>
            await _repository.GetWithUserDetailsByIdAsync(id);

        public async Task<Customer> AddCustomerWithSaveAsync(Customer customer)
        {
            var duplicate = await _repository.ExistsByPhoneNoAsync(customer.PhoneNo);
            if (duplicate)
                throw new Exception($"Customer with phoneNo '{customer.PhoneNo}' already exists.");

            await _repository.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Customer created: Id={Id}, PhoneNo={PhoneNo}", customer.CustomerID, customer.PhoneNo);
            return customer;
        }

        public async Task<Customer> UpdateCustomerWithSaveAsync(int id, Customer customer)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("Customer not found");

            var duplicate = await _repository.ExistsByPhoneNoAsync(customer.PhoneNo, id);
            if (duplicate)
                throw new Exception($"Customer with phoneNo '{customer.PhoneNo}' already exists.");

            var updatedCustomer = await _repository.UpdateAsync(id, customer);
            await _unitOfWork.SaveChangesAsync();

            if (updatedCustomer != null)
            {
                _logger.LogInformation("Customer updated: Id={Id}, PhoneNo={PhoneNo}", updatedCustomer.CustomerID, updatedCustomer.PhoneNo);
                return updatedCustomer;
            }

            throw new Exception("Customer update failed.");
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