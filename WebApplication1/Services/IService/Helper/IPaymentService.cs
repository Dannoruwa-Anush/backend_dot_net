using WebApplication1.DTOs.RequestDto.Payment;

namespace WebApplication1.Services.IService.Helper
{
    public interface IPaymentService
    {
        Task<bool> ProcessPaymentAsync(PaymentRequestDto paymentRequest);
    }
}