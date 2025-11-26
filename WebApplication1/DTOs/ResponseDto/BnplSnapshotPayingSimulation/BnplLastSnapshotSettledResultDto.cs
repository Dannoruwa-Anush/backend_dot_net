namespace WebApplication1.DTOs.ResponseDto.BnplSnapshotPayingSimulation
{
    public class BnplLastSnapshotSettledResultDto
    {
        public decimal TotalPaidArrears { get; set; } = 0m; // Arreas : previous Installment_Base
        public decimal TotalPaidLateInterest { get; set; } = 0m;
        public decimal TotalPaidCurrentInstallmentBase { get; set; } = 0m;
        public decimal OverPaymentCarriedToNextInstallment { get; set; } = 0m;
    }
}

