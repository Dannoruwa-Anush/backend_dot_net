using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;

namespace WebApplication1.Repositories.RepositoryImpl
{
    public class CategoryRepositoryImpl : ICategoryRepository
    {
        private readonly AppDbContext _context;

        // Constructor
        public CategoryRepositoryImpl(AppDbContext context)
        {
            // Dependency injection
            _context = context;
        }
        // Note : SaveChangesAsync() of Add, Update, Delete will be handled by UOW

        //CRUD operations
        public async Task<IEnumerable<Category>> GetAllAsync() =>
            await _context.Categories.ToListAsync();

        public async Task<Category?> GetByIdAsync(int id) =>
            await _context.Categories.FindAsync(id);

        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
        }

        public async Task<Category?> UpdateAsync(int id, Category category)
        {
            var existing = await _context.Categories.FindAsync(id);
            if (existing == null)
                return null;

            existing.CategoryName = category.CategoryName;
            _context.Categories.Update(existing);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return false;

            _context.Categories.Remove(category);
            return true;
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<Category>> GetAllWithPaginationAsync(int pageNumber, int pageSize, string? searchKey = null)
        {
            var query = _context.Categories.AsNoTracking().AsQueryable();

            // Apply filters from helper
            query = ApplyCategoryFilters(query, searchKey).OrderByDescending(c => c.CreatedAt);

            // Get total count after filtering
            var totalCount = await query.CountAsync();

            // Get paginated data
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginationResultDto<Category>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        //Helper method
        private IQueryable<Category> ApplyCategoryFilters(IQueryable<Category> query, string? searchKey)
        {
            if (!string.IsNullOrWhiteSpace(searchKey))
            {
                searchKey = searchKey.Trim();
                query = query.Where(c => EF.Functions.Like(c.CategoryName, $"%{searchKey}%"));
            }

            return query;
        }

        public async Task<bool> ExistsByCategoryNameAsync(string name)
        {
            return await _context.Categories.AnyAsync(c => c.CategoryName.ToLower() == name.ToLower());
        }

        public async Task<bool> ExistsByCategoryNameAsync(string name, int excludeId)
        {
            return await _context.Categories.AnyAsync(c => c.CategoryName.ToLower() == name.ToLower() && c.CategoryID != excludeId);
        }
    }
}