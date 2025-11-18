using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models.Base;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Models
{
    public class BNPL_PlanSettlementSummary : BaseModel //(In base model: CreatedAt, UpdatedAt)
    {
        [Key]
        public int SettlementID { get; set; }

        [Required]
        public int CurrentInstallmentNo { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCurrentArrears { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCurrentLateInterest { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal InstallmentBaseAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCurrentOverPayment { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPayableSettlement { get; set; }

        public DateTime? RefundDate { get; set; }

        [EnumDataType(typeof(BNPL_PlanSettlementSummary_StatusEnum))]
        [Column(TypeName = "nvarchar(30)")]
        public BNPL_PlanSettlementSummary_StatusEnum Bnpl_PlanSettlementSummary_Status { get; set; } = BNPL_PlanSettlementSummary_StatusEnum.Active;
        
        public bool IsLatest { get; set; }

        //******* [Start: BNPL_PLAN (1) — BNPL_PlanSettlementSummary (M)] ****
        //FK
        public int Bnpl_PlanID { get; set; }

        // Many Side: Navigation property
        [ForeignKey(nameof(Bnpl_PlanID))]
        [InverseProperty(nameof(BNPL_PLAN.BNPL_PlanSettlementSummaries))]
        public BNPL_PLAN? BNPL_PLAN { get; set; }
        //******* [End: BNPL_PLAN (1) — BNPL_PlanSettlementSummary (M)] ******
    }
}