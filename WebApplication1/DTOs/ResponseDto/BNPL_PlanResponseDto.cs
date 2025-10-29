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

        //FK 
        public int Bnpl_PlanTypeID { get; set; }

        //FK associated fields from BNPL_PlanType
        public string Bnpl_PlanTypeName { get; set; } = string.Empty;

        //FK
        public int OrderID { get; set; }
    }
}