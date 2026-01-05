using WebApplication1.DTOs.ResponseDto;

namespace WebApplication1.Services.IService
{
    public interface IDocumentGenerationService
    {
        Task<string> GenerateInvoicePdfAsync(CustomerOrderResponseDto order, InvoiceResponseDto invoice);
    }
}