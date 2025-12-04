using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.ResponseDto
{
    public class BNPL_InstallmentResponseDto
    {
        public int InstallmentID { get; set; }

        public int InstallmentNo { get; set; }

        public decimal Installment_BaseAmount { get; set; } 

        public DateTime Installment_DueDate { get; set; }

        public decimal OverpaymentCarriedToNext { get; set; } = 0m;

        public decimal LateInterest { get; set; } = 0m; 

        public decimal TotalDueAmount { get; set; }

        public decimal AmountPaid_AgainstBase { get; set; } = 0m;

        public decimal AmountPaid_AgainstLateInterest { get; set; } = 0m;

        public DateTime? LastPaymentDate { get; set; }

        public DateTime? RefundDate { get; set; }

        public DateTime? LastLateInterestAppliedDate { get; set; }

        public BNPL_Installment_StatusEnum Bnpl_Installment_Status { get; set; } = BNPL_Installment_StatusEnum.Pending;
        
        public decimal RemainingBalance => TotalDueAmount - (AmountPaid_AgainstBase + AmountPaid_AgainstLateInterest);
       
        // Include simplified info about FK: Bnpl_Plan
        public BNPL_PlanResponseDto? BNPL_PlanResponseDto { get; set; }
    }
}