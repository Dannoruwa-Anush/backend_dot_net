using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;

namespace WebApplication1.Repositories.RepositoryImpl
{
    public class BrandRepositoryImpl : IBrandRepository
    {
        private readonly AppDbContext _context;

        // Constructor
        public BrandRepositoryImpl(AppDbContext context)
        {
            // Dependency injection
            _context = context;
        }

        //CRUD operations
        public async Task<IEnumerable<Brand>> GetAllAsync() =>
            await _context.Brands.ToListAsync();

        public async Task<Brand?> GetByIdAsync(int id) =>
            await _context.Brands.FindAsync(id);

        public async Task AddAsync(Brand brand)
        {
            await _context.Brands.AddAsync(brand);
            await _context.SaveChangesAsync();
        }

        public async Task<Brand?> UpdateBrandAsync(int id, Brand brand)
        {
            var existing = await _context.Brands.FindAsync(id);
            if (existing == null)
                return null;

            existing.BrandName = brand.BrandName;
            _context.Brands.Update(existing);
            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
                return false;

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
            return true;
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<Brand>> GetAllWithPaginationAsync(int pageNumber, int pageSize, string? searchKey = null)
        {
            var query = _context.Brands.AsNoTracking().AsQueryable();

            // Apply filters from helper
            query = ApplyBrandFilters(query, searchKey).OrderByDescending(c => c.CreatedAt);

            // Get total count after filtering
            var totalCount = await query.CountAsync();

            // Get paginated data
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginationResultDto<Brand>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        //Helper method
        private IQueryable<Brand> ApplyBrandFilters(IQueryable<Brand> query, string? searchKey)
        {
            if (!string.IsNullOrWhiteSpace(searchKey))
            {
                searchKey = searchKey.Trim();
                query = query.Where(b => EF.Functions.Like(b.BrandName, $"%{searchKey}%"));
            }

            return query;
        }

        public async Task<bool> ExistsByBrandNameAsync(string name)
        {
            return await _context.Brands
                .AnyAsync(B => B.BrandName.ToLower() == name.ToLower());
        }

        public async Task<bool> ExistsByBrandNameAsync(string name, int excludeId)
        {
            return await _context.Brands
                .AnyAsync(B => B.BrandName.ToLower() == name.ToLower() && B.BrandID != excludeId);
        }
    }
}
