using WebApplication1.DTOs.RequestDto.BnplSnapshotPayingSimulation;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.IService
{
    public interface IInvoiceService
    {
        //CRUD operations
        Task<IEnumerable<Invoice>> GetAllInvoicesAsync();
        Task<Invoice?> GetInvoiceByIdAsync(int id);
        Task<Invoice?> GetInvoiceWithOrderAsync(int id);
        Task<Invoice?> GetInvoiceWithOrderFinancialDetailsAsync(int id);
        
        //Single Repository Operations (save immediately)
        Task<Invoice> UpdateInvoiceWithSaveAsync(int id);

        //Custom Query Operations
        Task<PaginationResultDto<Invoice>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? invoiceTypeId = null, int? invoiceStatusId = null, int? customerId = null, int? orderSourceId = null, string? searchKey = null);
        Task<bool> ExistsUnpaidInvoiceByCustomerAsync(int customerId);
        
        //Shared Internal Operations Used by Multiple Repositories
        Task<Invoice> BuildInvoiceAddRequestAsync(CustomerOrder order, InvoiceTypeEnum invoiceType);

        //Create an invoice for installment pay
        Task<Invoice> GenerateInvoiceForSettlementSimulationAsync(BnplSnapshotPayingSimulationRequestDto request);
    }
}