using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;

namespace WebApplication1.Services.ServiceImpl
{
    public class CustomerServiceImpl : ICustomerService
    {
        private readonly ICustomerRepository _repository;

        private readonly ICustomerOrderRepository _customerOrderRepository;

        //logger: for auditing
        private readonly ILogger<CustomerServiceImpl> _logger;

        // Constructor
        public CustomerServiceImpl(ICustomerRepository repository, ICustomerOrderRepository customerOrderRepository, ILogger<CustomerServiceImpl> logger)
        {
            // Dependency injection
            _repository              = repository;
            _customerOrderRepository = customerOrderRepository;
            _logger                  = logger;
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync() =>
            await _repository.GetAllAsync();


        public async Task<Customer?> GetCustomerByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        public async Task<Customer> AddCustomerAsync(Customer customer)
        {
            var duplicate = await _repository.ExistsByEmailAsync(customer.Email);
            if (duplicate)
                throw new Exception($"Customer with email '{customer.Email}' already exists.");

            await _repository.AddAsync(customer);

            _logger.LogInformation("Customer created: Id={Id}, Email={Email}", customer.CustomerID, customer.Email);
            return customer;
        }

        public async Task<Customer> UpdateCustomerAsync(int id, Customer customer)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("Customer not found");

            var duplicate = await _repository.ExistsByEmailAsync(customer.Email, id);
            if (duplicate)
                throw new Exception($"Customer with email '{customer.Email}' already exists.");

            var updatedCustomer = await _repository.UpdateAsync(id, customer);

            if (updatedCustomer != null)
            {
                _logger.LogInformation("Customer updated: Id={Id}, Email={Email}", updatedCustomer.CustomerID, updatedCustomer.Email);
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
            if (!deleted)
            {
                _logger.LogWarning("Attempted to delete customer with id {Id}, but it does not exist.", id);
                throw new Exception("Customer not found");
            }

            _logger.LogInformation("Customer deleted successfully: Id={Id}", id);
        }

        public async Task<PaginationResultDto<Customer>> GetAllWithPaginationAsync(int pageNumber, int pageSize)
        {
            return await _repository.GetAllWithPaginationAsync(pageNumber, pageSize);
        }
    }
}