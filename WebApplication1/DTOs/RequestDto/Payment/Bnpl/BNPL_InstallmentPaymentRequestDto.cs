namespace WebApplication1.DTOs.RequestDto.Payment.Bnpl
{
    public class BNPL_InstallmentPaymentRequestDto
    {
        public int OrderId { get; set; }
        public decimal PaymentAmount { get; set; }
    }
}