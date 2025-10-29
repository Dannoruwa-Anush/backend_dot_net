using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.ResponseDto
{
    public class BNPL_PlanResponseDto
    {
        public int Bnpl_PlanID { get; set; }

        public int Bnpl_TotalInstallments { get; set; }

        public decimal Bnpl_InstallmentAmount { get; set; }

        public double Bnpl_InterestRate { get; set; }

        public DateTime Bnpl_StartDate { get; set; }

        public DateTime Bnpl_NextDueDate { get; set; }

        public decimal Bnpl_RemainingBalance { get; set; }

        public BnplStatusEnum Bnpl_Status { get; set; } = BnplStatusEnum.Incomplete;


        // Include simplified info about FK: Bnpl_PlanType
        public required BNPL_PlanTypeResponseDto BNPL_PlanTypeResponseDto { get; set; }

        // Include simplified info about FK: Order
        public required CustomerOrderResponseDto  CustomerOrderResponseDto { get; set; }
    }
}