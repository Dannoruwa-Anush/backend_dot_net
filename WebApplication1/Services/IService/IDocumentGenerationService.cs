using WebApplication1.Models;

namespace WebApplication1.Services.IService
{
    public interface IDocumentGenerationService
    {
        Task<string> GenerateInvoicePdfAsync(CustomerOrder order, Invoice invoice);

        Task<string> GeneratePaymentReceiptPdfAsync(CustomerOrder order, Cashflow cashflow);
        Task<string> GenerateRefundReceiptPdfAsync(CustomerOrder order, Cashflow cashflow);
    }
}