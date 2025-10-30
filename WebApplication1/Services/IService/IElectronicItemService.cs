using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Services.IService
{
    public interface IElectronicItemService
    {
        Task<IEnumerable<ElectronicItem>> GetAllElectronicItemsAsync();
        Task<ElectronicItem?> GetElectronicItemByIdAsync(int id);
        Task<ElectronicItem> AddElectronicItemAsync(ElectronicItem electronicItem);
        Task<ElectronicItem> UpdateElectronicItemAsync(int id, ElectronicItem electronicItem);
        Task DeleteElectronicItemAsync(int id);
        Task<PaginationResultDto<ElectronicItem>> GetAllWithPaginationAsync(int pageNumber, int pageSize);
    }
}