using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Services.IService
{
    public interface IElectronicItemService
    {
        //CRUD operations
        Task<IEnumerable<ElectronicItem>> GetAllElectronicItemsAsync();
        Task<ElectronicItem?> GetElectronicItemByIdAsync(int id);

        //Single Repository Operations (save immediately)
        Task<ElectronicItem> AddElectronicItemWithSaveAsync(ElectronicItem electronicItem);
        Task<ElectronicItem> UpdateElectronicItemWithSaveAsync(int id, ElectronicItem electronicItem);
        Task DeleteElectronicItemWithSaveAsync(int id);

        //Custom Query Operations
        Task<PaginationResultDto<ElectronicItem>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? categoryId = null, int? brandId = null, string? searchKey = null);
        Task<IEnumerable<ElectronicItem>> GetAllElectronicItemsByCategoryIdAsync(int categoryId);
        Task<IEnumerable<ElectronicItem>> GetAllElectronicItemsByBrandIdAsync(int brandId);
        Task<List<ElectronicItem>> GetAllElectronicItemsByIdsAsync(List<int> ids);
    }
}