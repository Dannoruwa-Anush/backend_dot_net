using WebApplication1.DTOs.RequestDto.Payment;
using WebApplication1.Models;

namespace WebApplication1.Services.IService.Helper
{
    public interface IPaymentService
    {
        Task<Invoice> ProcessPaymentAsync(PaymentRequestDto paymentRequest);
    }
}