using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.ResponseDto
{
    public class BNPL_PlanSettlementSummaryResponseDto
    {
        public int SettlementID { get; set; }

        public int CurrentInstallmentNo { get; set; }

        public decimal TotalCurrentArrears { get; set; }

        public decimal TotalCurrentLateInterest { get; set; }

        public decimal InstallmentBaseAmount { get; set; }

        public decimal TotalCurrentOverPayment { get; set; }

        public decimal TotalPayableSettlement { get; set; }

        public BNPL_PlanSettlementSummary_StatusEnum Bnpl_PlanSettlementSummary_Status { get; set; } = BNPL_PlanSettlementSummary_StatusEnum.Active;
        
        public bool IsLatest { get; set; } = true;

        // Include simplified info about FK: Bnpl_Plan
        public BNPL_PlanResponseDto? BNPL_PlanResponseDto { get; set; }
    }
}