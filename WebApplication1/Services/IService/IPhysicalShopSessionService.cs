using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Services.IService
{
    public interface IPhysicalShopSessionService
    {
        //CRUD operations
        Task<IEnumerable<PhysicalShopSession>> GetAllPhysicalShopSessionsAsync();
        Task<PhysicalShopSession?> GetPhysicalShopSessionByIdAsync(int id);

        //Single Repository Operations (save immediately)
        Task<PhysicalShopSession> AddPhysicalShopSessionWithSaveAsync(PhysicalShopSession session);
        Task<PhysicalShopSession> UpdatePhysicalShopSessionWithSaveAsync(int id, PhysicalShopSession session);

        //Custom Query Operations
    }
}