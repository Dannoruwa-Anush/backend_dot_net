using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.RequestDto.BnplSnapshotPayingSimulation
{
    public class BnplSnapshotPayingSimulationRequestDto
    {
        public int OrderId { get; set; }
        public decimal PaymentAmount { get; set; }
        public InvoicePaymentChannelEnum  InvoicePaymentChannel { get; set; } = InvoicePaymentChannelEnum.ByVisitingShop;
    }
}