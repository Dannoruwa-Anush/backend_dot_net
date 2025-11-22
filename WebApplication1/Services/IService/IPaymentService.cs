using WebApplication1.DTOs.RequestDto.BnplCal;
using WebApplication1.DTOs.RequestDto.Payment;
using WebApplication1.DTOs.ResponseDto.Payment.Bnpl;

namespace WebApplication1.Services.IService
{
    public interface IPaymentService
    {
        Task ProcessFullPaymentPaymentAsync(PaymentRequestDto paymentRequest);
        Task ProcessBnplInitialPaymentAsync(BNPLInstallmentCalculatorRequestDto request);
        Task<BnplInstallmentPaymentResultDto> ProcessBnplInstallmentPaymentAsync(PaymentRequestDto paymentRequest);
        
        //need to update all associated child fileds : Refund (Order cancel): Payment Complete
        //Task<CustomerOrder?> BuildPaymentUpdateRequestAsync(CustomerOrderPaymentStatusChangeRequestDto request);
    }
}