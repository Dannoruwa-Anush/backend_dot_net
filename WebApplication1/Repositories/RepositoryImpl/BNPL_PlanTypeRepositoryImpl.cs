using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;

namespace WebApplication1.Repositories.RepositoryImpl
{
    public class BNPL_PlanTypeRepositoryImpl : IBNPL_PlanTypeRepository
    {
        private readonly AppDbContext _context;

        // Constructor
        public BNPL_PlanTypeRepositoryImpl(AppDbContext context)
        {
            // Dependency injection
            _context = context;
        }

        public async Task<IEnumerable<BNPL_PlanType>> GetAllAsync() =>
            await _context.BNPL_PlanTypes.ToListAsync();

        public async Task<BNPL_PlanType?> GetByIdAsync(int id) =>
            await _context.BNPL_PlanTypes.FindAsync(id);

        public async Task AddAsync(BNPL_PlanType bNPL_PlanType) =>
            await _context.BNPL_PlanTypes.AddAsync(bNPL_PlanType);

        public async Task<BNPL_PlanType?> UpdateAsync(int id, BNPL_PlanType bNPL_PlanType)
        {
            var existing = await _context.BNPL_PlanTypes.FindAsync(id);
            if (existing == null) 
                return null;

            existing.Bnpl_PlanTypeName = bNPL_PlanType.Bnpl_PlanTypeName;
            existing.Bnpl_DurationDays = bNPL_PlanType.Bnpl_DurationDays;
            existing.Bnpl_Description = bNPL_PlanType.Bnpl_Description;

            _context.BNPL_PlanTypes.Update(bNPL_PlanType);
            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var bnpl_PlanType = await _context.BNPL_PlanTypes.FindAsync(id);
            if (bnpl_PlanType == null) 
                return false;

            _context.BNPL_PlanTypes.Remove(bnpl_PlanType);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<PaginationResultDto<BNPL_PlanType>> GetAllWithPaginationAsync(int pageNumber, int pageSize)
        {
            var totalCount = await _context.BNPL_PlanTypes.CountAsync();

            var items = await _context.BNPL_PlanTypes
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return new PaginationResultDto<BNPL_PlanType>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        
        public async Task<bool> ExistsByBNPL_PlanTypeNameAsync(string name)
        {
            return await _context.BNPL_PlanTypes.AnyAsync(bnpTy => bnpTy.Bnpl_PlanTypeName.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<bool> ExistsByBNPL_PlanTypeNameAsync(string name, int excludeId)
        {
             return await _context.BNPL_PlanTypes.AnyAsync(bnpTy => bnpTy.Bnpl_PlanTypeName.Equals(name, StringComparison.OrdinalIgnoreCase) && bnpTy.Bnpl_PlanTypeID != excludeId);
        }
    }
}