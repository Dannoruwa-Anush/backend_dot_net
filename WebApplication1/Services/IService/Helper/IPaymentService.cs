using WebApplication1.DTOs.RequestDto.BnplCal;
using WebApplication1.DTOs.RequestDto.Payment;
using WebApplication1.DTOs.ResponseDto.Payment.Bnpl;

namespace WebApplication1.Services.IService.Helper
{
    public interface IPaymentService
    {
        Task<BnplInstallmentPaymentResultDto?> ProcessPaymentAsync(PaymentRequestDto paymentRequest);
    }
}