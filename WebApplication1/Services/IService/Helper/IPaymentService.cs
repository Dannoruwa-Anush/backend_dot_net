using WebApplication1.DTOs.RequestDto.Payment;
using WebApplication1.DTOs.ResponseDto.Payment.Bnpl;
using WebApplication1.Models;

namespace WebApplication1.Services.IService.Helper
{
    public interface IPaymentService
    {
        Task<bool> ProcessFullPaymentAsync(PaymentRequestDto paymentRequest);
        Task<BNPL_PLAN> ProcessInitialBnplPaymentAsync(BnplInitialPaymentRequestDto paymentRequest);
        Task<BnplInstallmentPaymentResultDto?> ProcessBnplInstallmentPaymentAsync(PaymentRequestDto paymentRequest);
    }
}