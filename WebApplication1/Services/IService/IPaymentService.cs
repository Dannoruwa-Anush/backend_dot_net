using WebApplication1.DTOs.RequestDto.BnplCal;
using WebApplication1.DTOs.RequestDto.Payment;
using WebApplication1.DTOs.ResponseDto.Payment.Bnpl;
using WebApplication1.Models;

namespace WebApplication1.Services.IService
{
    public interface IPaymentService
    {
        Task ProcessFullPaymentPaymentAsync(PaymentRequestDto paymentRequest);
        Task ProcessBnplInitialPaymentAsync(BNPLInstallmentCalculatorRequestDto request);
        Task<BnplInstallmentPaymentResultDto> ProcessBnplInstallmentPaymentAsync(PaymentRequestDto paymentRequest);
        
        // Need to update assossiated fields of Order (cashflow : refunds, BNPL_Plan : Cancel, Installment : Refund, Snapshot : Cancelled)  
        Task BuildPaymentRefundUpdateRequestAsync(CustomerOrder order, DateTime now);
    }
}