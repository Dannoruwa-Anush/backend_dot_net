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

        public async Task<IEnumerable<Category>> GetAllAsync() =>
            await _context.Categories.ToListAsync();

        public async Task<Category?> GetByIdAsync(int id) =>
            await _context.Categories.FindAsync(id);

        public async Task AddAsync(Category category) =>
            await _context.Categories.AddAsync(category);

        public async Task<Category?> UpdateAsync(int id, Category category)
        {
            var existing = await _context.Categories.FindAsync(id);
            if (existing == null) 
                return null;

            existing.CategoryName = category.CategoryName;
            _context.Categories.Update(existing);
            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) 
                return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PaginationResultDto<Category>> GetAllWithPaginationAsync(int pageNumber, int pageSize)
        {
            var totalCount = await _context.Categories.CountAsync();

            var items = await _context.Categories
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

        public async Task<bool> ExistsByCategoryNameAsync(string name)
        {
            return await _context.Categories.AnyAsync(c => c.CategoryName.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<bool> ExistsByCategoryNameAsync(string name, int excludeId)
        {
            return await _context.Categories.AnyAsync(c => c.CategoryName.Equals(name, StringComparison.OrdinalIgnoreCase) && c.CategoryID != excludeId);
        }
    }
}