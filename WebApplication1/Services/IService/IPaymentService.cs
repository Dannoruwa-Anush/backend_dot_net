using WebApplication1.DTOs.RequestDto.Payment;

namespace WebApplication1.Services.IService
{
    public interface IPaymentService
    {
        Task ProcessFullPaymentPaymentAsync(PaymentRequestDto paymentRequest);
        //Task ProcessBnplInitialPaymentAsync(PaymentRequestDto paymentRequest);
        Task ProcessBnplInstallmentPaymentAsync(PaymentRequestDto paymentRequest);
        //cancel payment
        //refund payment
    }
}