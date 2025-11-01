namespace WebApplication1.DTOs.RequestDto.BnplPaymentSimulation
{
    public class BnplInstallmentPaymentSimulationRequestDto
    {
        public int InstallmentId { get; set; }
        public decimal PaymentAmount { get; set; }
    }
}