namespace WebApplication1.DTOs.ResponseDto.Payment.Bnpl
{
    public class BnplInstallmentPaymentResultDto
    {
        public int InstallmentId { get; set; }
        public decimal AppliedToArrears { get; set; }
        public decimal AppliedToLateInterest { get; set; }
        public decimal AppliedToBase { get; set; }
        public decimal OverPayment { get; set; }
        public string NewStatus { get; set; } = "";
        public List<BnplPerInstallmentPaymentBreakdownResultDto> PerInstallmentBreakdown { get; set; } = new List<BnplPerInstallmentPaymentBreakdownResultDto>();
    }
}