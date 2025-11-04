using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Services.IService
{
    public interface IElectronicItemService
    {
        //CRUD operations
        Task<IEnumerable<ElectronicItem>> GetAllElectronicItemsAsync();
        Task<ElectronicItem?> GetElectronicItemByIdAsync(int id);
        Task<ElectronicItem> AddElectronicItemAsync(ElectronicItem electronicItem);
        Task<ElectronicItem> UpdateElectronicItemAsync(int id, ElectronicItem electronicItem);
        Task DeleteElectronicItemAsync(int id);

        //Custom Query Operations
        Task<PaginationResultDto<ElectronicItem>> GetAllWithPaginationAsync(int pageNumber, int pageSize, string? searchKey = null);
        Task<IEnumerable<ElectronicItem>> GetAllElectronicItemsByCategoryIdAsync(int categoryId);
        Task<IEnumerable<ElectronicItem>> GetAllElectronicItemsByBrandIdAsync(int brandId);
    }
}