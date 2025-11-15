namespace WebApplication1.DTOs.ResponseDto.BnplPaymentSimulation
{
    public class BnplInstallmentPaymentSimulationResultDto
    {
        public int InstallmentId { get; set; }
        public decimal InputPayment { get; set; }

        public decimal PaidToArrears { get; set; }
        public decimal PaidToInterest { get; set; }
        public decimal PaidToBase { get; set; }

        public decimal RemainingBalance { get; set; }
        public decimal OverPaymentCarried { get; set; }

        public string ResultStatus { get; set; } = "";

        public List<BnplInstallmentPaymentSimulationResultDto> PerInstallmentBreakdown { get; set; } = new List<BnplInstallmentPaymentSimulationResultDto>();
    }

}