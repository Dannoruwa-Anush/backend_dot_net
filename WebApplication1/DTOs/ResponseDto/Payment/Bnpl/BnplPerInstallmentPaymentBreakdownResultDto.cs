namespace WebApplication1.DTOs.ResponseDto.Payment.Bnpl
{
    public class BnplPerInstallmentPaymentBreakdownResultDto
    {
        public int InstallmentId { get; set; }
        public decimal AppliedToArrears { get; set; }
        public decimal AppliedToLateInterest { get; set; }
        public decimal AppliedToBase { get; set; }
        public decimal OverPayment { get; set; }
        public string NewStatus { get; set; } = "";
    }
}