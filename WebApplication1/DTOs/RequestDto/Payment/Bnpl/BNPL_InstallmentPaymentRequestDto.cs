namespace WebApplication1.DTOs.RequestDto.Payment.Bnpl
{
    public class BNPL_InstallmentPaymentRequestDto
    {
        public int OrderId { get; set; }
        public decimal PayingArrears { get; set; }
        public decimal PayingLateInterest { get; set; }
        public decimal PayingBase { get; set; }
        public decimal OverPayment { get; set; }
    }
}