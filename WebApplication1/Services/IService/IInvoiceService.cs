using WebApplication1.DTOs.RequestDto;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Services.IService
{
    public interface IInvoiceService
    {
        //CRUD operations
        Task<IEnumerable<Invoice>> GetAllInvoicesAsync();
        Task<Invoice?> GetInvoiceByIdAsync(int id);

        //Custom Query Operations
        Task<PaginationResultDto<Invoice>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? invoiceTypeId = null, int? invoiceStatusId = null, string? searchKey = null);

        //Shared Internal Operations Used by Multiple Repositories
        Task<Invoice> BuildInvoiceAddRequestAsync(CustomerOrder order, CustomerOrderRequestDto request);
        Task<Invoice> CreateInstallmentInvoiceAsync(CustomerOrder order, int installmentNo);
    }
}