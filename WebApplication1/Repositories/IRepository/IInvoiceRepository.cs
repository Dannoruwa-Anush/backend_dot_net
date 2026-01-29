using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Repositories.IRepository
{
    public interface IInvoiceRepository
    {
        //CRUD operations
        Task<IEnumerable<Invoice>> GetAllAsync();
        Task<Invoice?> GetByIdAsync(int id);
        Task<Invoice?> GetInvoiceWithOrderAsync(int invoiceId);
        Task<Invoice?> GetInvoiceWithOrderFinancialDetailsAsync(int invoiceId);
        Task<Invoice?> UpdateAsync(int id, Invoice invoice);

        //Custom Query Operations
        Task<PaginationResultDto<Invoice>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? invoiceTypeId = null, int? invoiceStatusId = null, int? customerId = null, int? orderSourceId = null, string? searchKey = null);
        Task<bool> ExistsUnpaidInvoiceByCustomerAsync(int customerId);
    }
}