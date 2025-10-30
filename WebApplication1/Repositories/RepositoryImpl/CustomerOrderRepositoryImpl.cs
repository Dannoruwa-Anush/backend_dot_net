using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;

namespace WebApplication1.Repositories.RepositoryImpl
{
    public class CustomerOrderRepositoryImpl : ICustomerOrderRepository
    {
        public Task<IEnumerable<CustomerOrder>> GetAllAsync()
        {
            throw new NotImplementedException();
        }
    }
}