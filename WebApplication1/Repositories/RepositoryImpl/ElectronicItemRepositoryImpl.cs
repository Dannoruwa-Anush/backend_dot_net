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

        public async Task<IEnumerable<ElectronicItem>> GetAllAsync() =>
            await _context.ElectronicItems
                .Include(e => e.Brand)
                .Include(e => e.Category)
                .ToListAsync();

        public async Task<ElectronicItem?> GetByIdAsync(int id) =>
            await _context.ElectronicItems
                .Include(e => e.Brand)
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.E_ItemID == id);

        public async Task AddAsync(ElectronicItem electronicItem)
        {
            await _context.ElectronicItems.AddAsync(electronicItem);
            await _context.SaveChangesAsync();
        }

        public async Task<ElectronicItem?> UpdateAsync(int id, ElectronicItem electronicItem)
        {
            var existing = await _context.ElectronicItems.FindAsync(id);
            if (existing == null)
                return null;

            existing.E_ItemName = electronicItem.E_ItemName;
            existing.Price = electronicItem.Price;
            existing.QOH = electronicItem.QOH;
            existing.BrandId = electronicItem.BrandId;
            existing.CategoryID = electronicItem.CategoryID;

            _context.ElectronicItems.Update(existing);
            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var e_item = await _context.ElectronicItems.FindAsync(id);
            if (e_item == null)
                return false;

            _context.ElectronicItems.Remove(e_item);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<PaginationResultDto<ElectronicItem>> GetAllWithPaginationAsync(int pageNumber, int pageSize)
        {
            var totalCount = await _context.ElectronicItems.CountAsync();

            var items = await _context.ElectronicItems
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

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.ElectronicItems.AnyAsync(i => i.E_ItemName.ToLower() == name.ToLower());
        }

        public async Task<bool> ExistsByNameAsync(string name, int excludeId)
        {
            return await _context.ElectronicItems.AnyAsync(i => i.E_ItemName.ToLower() == name.ToLower() && i.E_ItemID != excludeId);
        }
    }
}