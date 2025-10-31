using WebApplication1.Models;

namespace WebApplication1.Repositories.IRepository
{
    public interface ICustomerOrderElectronicItemRepository
    {
        //CRUD operations
        Task AddAsync(CustomerOrderElectronicItem orderItem);
    }
}