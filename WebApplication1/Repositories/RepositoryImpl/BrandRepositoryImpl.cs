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

        public BrandRepositoryImpl(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Brand>> GetAllAsync() =>
            await _context.Brands.ToListAsync();

        public async Task<Brand?> GetByIdAsync(int id) =>
            await _context.Brands.FindAsync(id);

        public async Task AddAsync(Brand brand) =>
            await _context.Brands.AddAsync(brand);

        public async Task<Brand?> UpdateBrandAsync(int id, Brand brand)
        {
            var existing = await _context.Brands.FindAsync(id);
            if (existing == null) return null;

            existing.BrandName = brand.BrandName;
            _context.Brands.Update(existing);
            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) return false;

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        //update: with transaction handling
        public async Task<Brand> UpdateBrandWithTransactionAsync(int id, Brand brand)
        {
            var existing = await _context.Brands.FindAsync(id);
            if (existing == null) throw new Exception("Brand not found");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                existing.BrandName = brand.BrandName;
                _context.Brands.Update(existing);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return existing;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<PaginationResultDto<Brand>> GetAllWithPaginationAsync(int pageNumber, int pageSize)
        {
            var totalCount = await _context.Brands.CountAsync();

            var items = await _context.Brands
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
    }
}
