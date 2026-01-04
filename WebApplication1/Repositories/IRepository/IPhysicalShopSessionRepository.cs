using WebApplication1.DTOs.ResponseDto.Common;
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
    }
}