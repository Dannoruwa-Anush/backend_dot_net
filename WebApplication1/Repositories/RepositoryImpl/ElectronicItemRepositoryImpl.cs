using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;

namespace WebApplication1.Repositories.RepositoryImpl
{
    public class ElectronicItemRepositoryImpl : IElectronicItemRepository
    {
        private readonly AppDbContext _context;

        // Constructor
        public ElectronicItemRepositoryImpl(AppDbContext context)
        {
            // Dependency injection
            _context = context;
        }
        // Note : SaveChangesAsync() of Add, Update, Delete will be handled by UOW

        //CRUD operations
        public async Task<IEnumerable<ElectronicItem>> GetAllAsync() =>
            await _context.ElectronicItems.ToListAsync();

        public async Task<IEnumerable<ElectronicItem>> GetAllWithBrandCategoryDetailsAsync() =>
            await _context.ElectronicItems
                .Include(e => e.Brand)
                .Include(e => e.Category)
                .ToListAsync();

        public async Task<ElectronicItem?> GetByIdAsync(int id) =>
            await _context.ElectronicItems.FindAsync(id);

        public async Task<ElectronicItem?> GetWithBrandCategoryDetailsByIdAsync(int id) =>
            await _context.ElectronicItems
                .Include(e => e.Brand)
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.ElectronicItemID == id);

        public async Task AddAsync(ElectronicItem electronicItem) =>
            await _context.ElectronicItems.AddAsync(electronicItem);

        public async Task<ElectronicItem?> UpdateAsync(int id, ElectronicItem electronicItem)
        {
            var existing = await _context.ElectronicItems.FindAsync(id);
            if (existing == null)
                return null;

            existing.ElectronicItemName = electronicItem.ElectronicItemName;
            existing.Price = electronicItem.Price;
            existing.QOH = electronicItem.QOH;
            existing.BrandID = electronicItem.BrandID;
            existing.CategoryID = electronicItem.CategoryID;

            _context.ElectronicItems.Update(existing);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var e_item = await _context.ElectronicItems.FindAsync(id);
            if (e_item == null)
                return false;

            _context.ElectronicItems.Remove(e_item);
            return true;
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<ElectronicItem>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? categoryId = null, int? brandId = null, string? searchKey = null)
        {
            var query = _context.ElectronicItems.AsNoTracking().AsQueryable();

            // Apply search filter
            query = ApplyElectronicItemFilters(query, searchKey);

            // Apply Category filter if provided
            if (categoryId.HasValue)
            {
                query = query.Where(e => e.CategoryID == categoryId.Value);
            }

            // Apply Brand filter if provided
            if (brandId.HasValue)
            {
                query = query.Where(e => e.BrandID == brandId.Value);
            }

            // Order by CreatedAt descending
            query = query.OrderByDescending(e => e.CreatedAt);

            var totalCount = await query.CountAsync();

            var items = await query
                .Include(e => e.Brand)
                .Include(e => e.Category)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginationResultDto<ElectronicItem>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        //Helper method
        private IQueryable<ElectronicItem> ApplyElectronicItemFilters(IQueryable<ElectronicItem> query, string? searchKey)
        {
            if (!string.IsNullOrWhiteSpace(searchKey))
            {
                searchKey = searchKey.Trim();
                query = query.Where(i => EF.Functions.Like(i.ElectronicItemName, $"%{searchKey}%"));
            }

            return query;
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.ElectronicItems.AnyAsync(i => i.ElectronicItemName.ToLower() == name.ToLower());
        }

        public async Task<bool> ExistsByNameAsync(string name, int excludeId)
        {
            return await _context.ElectronicItems.AnyAsync(i => i.ElectronicItemName.ToLower() == name.ToLower() && i.ElectronicItemID != excludeId);
        }

        public async Task<IEnumerable<ElectronicItem>> GetAllByCategoryAsync(int categoryId)
        {
            return await _context.ElectronicItems
                .Include(e => e.Brand)
                .Include(e => e.Category)
                .Where(e => e.CategoryID == categoryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ElectronicItem>> GetAllByBrandAsync(int brandId)
        {
            return await _context.ElectronicItems
                .Include(e => e.Brand)
                .Include(e => e.Category)
                .Where(e => e.BrandID == brandId)
                .ToListAsync();
        }

        public async Task<bool> ExistsByCategoryAsync(int categoryId)
        {
            return await _context.ElectronicItems.AnyAsync(e => e.CategoryID == categoryId);
        }

        public async Task<bool> ExistsByBrandAsync(int brandId)
        {
            return await _context.ElectronicItems.AnyAsync(e => e.BrandID == brandId);
        }

        public async Task<List<ElectronicItem>> GetAllByIdsAsync(List<int> ids)
        {
            return await _context.ElectronicItems
                .Where(e => ids.Contains(e.ElectronicItemID))
                .ToListAsync();
        }
    }
}