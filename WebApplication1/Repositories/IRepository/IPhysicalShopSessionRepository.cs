using WebApplication1.Models;

namespace WebApplication1.Repositories.IRepository
{
    public interface IPhysicalShopSessionRepository
    {
        //CRUD operations
        Task<IEnumerable<PhysicalShopSession>> GetAllAsync();
        Task<PhysicalShopSession?> GetByIdAsync(int id);
        Task AddAsync(PhysicalShopSession physicalShopSession);
        Task<PhysicalShopSession?> UpdateAsync(int id, PhysicalShopSession physicalShopSession);
        
        //Custom Query Operations
        Task<PhysicalShopSession?> GetActiveSessionAsync();
        Task<bool> HasActiveSessionAsync();
        Task<PhysicalShopSession?> GetActiveSessionForTodayAsync();
        Task<PhysicalShopSession?> GetLatestActiveSessionAsync();
    }
}