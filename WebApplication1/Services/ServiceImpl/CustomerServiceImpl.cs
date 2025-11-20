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

        private readonly ICustomerOrderRepository _customerOrderRepository;

        //logger: for auditing
        private readonly ILogger<CustomerServiceImpl> _logger;

        // Constructor
        public CustomerServiceImpl(ICustomerRepository repository, IAppUnitOfWork unitOfWork, ICustomerOrderRepository customerOrderRepository, ILogger<CustomerServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _unitOfWork = unitOfWork;
            _customerOrderRepository = customerOrderRepository;
            _logger = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<Customer>> GetAllCustomersAsync() =>
            await _repository.GetAllAsync();


        public async Task<Customer?> GetCustomerByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        public async Task<Customer> AddCustomerAsync(Customer customer)
        {
            var duplicate = await _repository.ExistsByPhoneNoAsync(customer.PhoneNo);
            if (duplicate)
                throw new Exception($"Customer with phoneNo '{customer.PhoneNo}' already exists.");

            await _repository.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Customer created: Id={Id}, PhoneNo={PhoneNo}", customer.CustomerID, customer.PhoneNo);
            return customer;
        }

        public async Task<Customer> UpdateCustomerAsync(int id, Customer customer)
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

        public async Task DeleteCustomerAsync(int id)
        {
            // Check if any CustomerOrders reference this customer
            bool hasItems = await _customerOrderRepository.ExistsByCustomerAsync(id);
            if (hasItems)
            {
                _logger.LogWarning("Cannot delete customer {Id} — associated customer orders exist.", id);
                throw new InvalidOperationException("Cannot delete this customer because customer orders are associated with it.");
            }

            // Proceed with deletion if safe
            var deleted = await _repository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            
            if (!deleted)
            {
                _logger.LogWarning("Attempted to delete customer with id {Id}, but it does not exist.", id);
                throw new Exception("Customer not found");
            }

            _logger.LogInformation("Customer deleted successfully: Id={Id}", id);
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<Customer>> GetAllWithPaginationAsync(int pageNumber, int pageSize, string? searchKey = null)
        {
            return await _repository.GetAllWithPaginationAsync(pageNumber, pageSize, searchKey);
        }
    }
}