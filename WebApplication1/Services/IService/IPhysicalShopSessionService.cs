using WebApplication1.Models;

namespace WebApplication1.Services.IService
{
    public interface IPhysicalShopSessionService
    {
        //CRUD operations
        Task<IEnumerable<PhysicalShopSession>> GetAllPhysicalShopSessionsAsync();
        Task<PhysicalShopSession?> GetPhysicalShopSessionByIdAsync(int id);

        //Single Repository Operations (save immediately)
        Task<PhysicalShopSession> AddPhysicalShopSessionWithSaveAsync();
        
        //Multiple Repository Operations (transactional)
        Task<PhysicalShopSession> ModifyPhysicalShopSessionWithTransactionAsync(int id);

        //Custom Query Operations
        Task<PhysicalShopSession?> GetActivePhysicalShopSessionForTodayAsync();
        Task<PhysicalShopSession?> GetLatestActivePhysicalShopSessionAsync();
    }
}