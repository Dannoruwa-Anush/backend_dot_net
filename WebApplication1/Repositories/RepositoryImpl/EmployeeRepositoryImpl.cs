using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Repositories.RepositoryImpl
{
    public class EmployeeRepositoryImpl : IEmployeeRepository
    {
        private readonly AppDbContext _context;

        // Constructor
        public EmployeeRepositoryImpl(AppDbContext context)
        {
            // Dependency injection
            _context = context;
        }
        // Note : SaveChangesAsync() of Add, Update, Delete will be handled by UOW

        //CRUD operations
        public async Task<IEnumerable<Employee>> GetAllAsync() =>
            await _context.Employees.ToListAsync();

        public async Task<IEnumerable<Employee>> GetAllWithUserDetailsAsync() =>
            await _context.Employees
                .Include(em => em.User)
                .ToListAsync();    

        public async Task<Employee?> GetByIdAsync(int id) =>
            await _context.Employees.FindAsync(id);
            
        public async Task<Employee?> GetWithUserDetailsByIdAsync(int id) =>
            await _context.Employees
                    .Include(em => em.User)
                    .FirstOrDefaultAsync(em => em.EmployeeID == id);

        public async Task AddAsync(Employee employee) =>
            await _context.AddAsync(employee);

        public async Task<Employee?> UpdateAsync(int id, Employee employee)
        {
            var existing = await _context.Employees.FindAsync(id);
            if (existing == null)
                return null;

            existing.EmployeeName = employee.EmployeeName;
            existing.Position = employee.Position;    

            _context.Employees.Update(existing);
            return existing;
        }

        public async Task<Employee?> UpdateProfileAsync(int id, Employee employee)
        {
            var existing = await _context.Employees.FindAsync(id);
            if (existing == null)
                return null;

            existing.EmployeeName = employee.EmployeeName;  

            _context.Employees.Update(existing);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return false;

            _context.Employees.Remove(employee);
            return true;
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<Employee>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? positionId, string? searchKey = null)
        {
            var query = _context.Employees.AsNoTracking().AsQueryable();

            // Apply filters from helper
            query = ApplyEmployeePositionFilter(query, positionId);
            query = ApplyEmployeeFilters(query, searchKey);

            query = query.OrderByDescending(c => c.CreatedAt);

            // Get total count after filtering
            var totalCount = await query.CountAsync();

            // Get paginated data
            var items = await query
                .Include(em => em.User)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginationResultDto<Employee>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        //Helper method: filter by employee position type
        private IQueryable<Employee> ApplyEmployeePositionFilter(IQueryable<Employee> query, int? positionId)
        {
            if (positionId.HasValue)
            {
                var status = (EmployeePositionEnum)positionId.Value;
                query = query.Where(t => t.Position == status);
            }
            return query;
        }

        //Helper method: filter by employee name
        private IQueryable<Employee> ApplyEmployeeFilters(IQueryable<Employee> query, string? searchKey)
        {
            if (!string.IsNullOrWhiteSpace(searchKey))
            {
                searchKey = searchKey.Trim();
                query = query.Where(e => EF.Functions.Like(e.EmployeeName, $"%{searchKey}%"));
            }

            return query;
        }

        public async Task<Employee?> GetByUserIdAsync(int userId)
        {
            return await _context.Set<Employee>()
                             .Include(e => e.User)
                             .FirstOrDefaultAsync(e => e.UserID == userId);
        }
    }
}