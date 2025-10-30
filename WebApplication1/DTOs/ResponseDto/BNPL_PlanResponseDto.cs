using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.ResponseDto
{
    public class BNPL_PlanResponseDto
    {
        public int Bnpl_PlanID { get; set; }

        public decimal Bnpl_AmountPerInstallment { get; set; }

        public int Bnpl_TotalInstallmentCount { get; set; }

        public int Bnpl_RemainingInstallmentCount { get; set; }

        public double Bnpl_InterestRate { get; set; }

        public DateTime Bnpl_StartDate { get; set; }

        public DateTime Bnpl_NextDueDate { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        public BnplStatusEnum Bnpl_Status { get; set; } = BnplStatusEnum.Active;

        // Include simplified info about FK: Bnpl_PlanType
        public required BNPL_PlanTypeResponseDto BNPL_PlanTypeResponseDto { get; set; }

        // Include simplified info about FK: Order
        public required CustomerOrderResponseDto  CustomerOrderResponseDto { get; set; }
    }
}