using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;

namespace WebApplication1.Repositories.RepositoryImpl
{
    public class BNPL_InstallmentRepositoryImpl : IBNPL_InstallmentRepository
    {
        private readonly AppDbContext _context;

        // Constructor
        public BNPL_InstallmentRepositoryImpl(AppDbContext context)
        {
            // Dependency injection
            _context = context;
        }

        //CRUD operations
        public async Task<IEnumerable<BNPL_Installment>> GetAllAsync() =>
            await _context.BNPL_Installments.ToListAsync();

        public async Task<BNPL_Installment?> GetByIdAsync(int id) =>
            await _context.BNPL_Installments.FindAsync(id);

        public async Task AddAsync(BNPL_Installment bnpl_installment)
        {
            await _context.BNPL_Installments.AddAsync(bnpl_installment);
            await _context.SaveChangesAsync();
        }

        public Task<BNPL_Installment?> UpdateAsync(int id, BNPL_Installment bnpl_installment)
        {
            throw new NotImplementedException();
        }
 
        //Custom Query Operations
        public Task<PaginationResultDto<BNPL_Installment>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? installmentStatusId = null, string? searchKey = null)
        {
            throw new NotImplementedException();
        }
    }
}