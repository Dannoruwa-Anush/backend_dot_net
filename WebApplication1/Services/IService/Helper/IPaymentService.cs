using WebApplication1.DTOs.RequestDto.Payment;

namespace WebApplication1.Services.IService.Helper
{
    public interface IPaymentService
    {
        Task<bool> ProcessPaymentAsync(PaymentRequestDto paymentRequest);
        //Task<bool> ProcessFullPaymentAsync(PaymentRequestDto paymentRequest);
        //Task<bool> ProcessInitialBnplPaymentAsync(BnplInitialPaymentRequestDto paymentRequest);
        //Task<bool> ProcessBnplInstallmentPaymentAsync(PaymentRequestDto paymentRequest);
    }
}