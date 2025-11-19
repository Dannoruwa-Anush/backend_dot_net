using WebApplication1.DTOs.RequestDto.BnplCal;
using WebApplication1.DTOs.RequestDto.Payment;

namespace WebApplication1.Services.IService
{
    public interface IPaymentService
    {
        Task ProcessFullPaymentPaymentAsync(PaymentRequestDto paymentRequest);
        Task ProcessBnplInitialPaymentAsync(BNPLInstallmentCalculatorRequestDto request);
        Task ProcessBnplInstallmentPaymentAsync(PaymentRequestDto paymentRequest);
        //cancel payment
        //refund payment
    }
}