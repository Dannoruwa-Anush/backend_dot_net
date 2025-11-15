namespace WebApplication1.DTOs.RequestDto.BnplPaymentSimulation
{
    public class BnplInstallmentPaymentSimulationRequestDto
    {
        public int OrderId { get; set; }
        public decimal PaymentAmount { get; set; }
    }
}