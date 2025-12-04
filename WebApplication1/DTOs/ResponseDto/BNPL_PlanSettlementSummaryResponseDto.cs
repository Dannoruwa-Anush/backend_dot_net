using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.ResponseDto
{
    public class BNPL_PlanSettlementSummaryResponseDto
    {
        public int SettlementID { get; set; }

        public int CurrentInstallmentNo { get; set; }

        public decimal NotYetDueCurrentInstallmentBaseAmount { get; set; } // Unpaid base for installments NOT YET DUE

        public decimal Total_InstallmentBaseArrears { get; set; } // Unpaid base for installments that are past due date

        public decimal Total_LateInterest { get; set; }

        public decimal Total_PayableSettlement { get; set; } 

        public decimal Paid_AgainstNotYetDueCurrentInstallmentBaseAmount { get; set; } = 0m;

        public decimal Paid_AgainstTotalArrears { get; set; } = 0m;

        public decimal Paid_AgainstTotalLateInterest { get; set; } = 0m;
        
        public decimal Total_OverpaymentCarriedToNext { get; set; } = 0m;

        public string Bnpl_PlanSettlementSummaryRef { get; set; } = string.Empty;

        public BNPL_PlanSettlementSummary_StatusEnum Bnpl_PlanSettlementSummary_Status { get; set; } = BNPL_PlanSettlementSummary_StatusEnum.Active;
        
        public bool IsLatest { get; set; } = true;

        public Bnpl_PlanSettlementSummary_PaymentStatusEnum Bnpl_PlanSettlementSummary_PaymentStatus { get; set; } = Bnpl_PlanSettlementSummary_PaymentStatusEnum.Unsettled;

        // Include simplified info about FK: Bnpl_Plan
        public BNPL_PlanResponseDto? BNPL_PlanResponseDto { get; set; }
    }
}