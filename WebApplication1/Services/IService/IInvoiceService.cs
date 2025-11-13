using WebApplication1.DTOs.ResponseDto;

namespace WebApplication1.Services.IService
{
    public interface IInvoiceService
    {
        Task<string> GenerateInvoicePdfAsync(CustomerOrderResponseDto order);
    }
}