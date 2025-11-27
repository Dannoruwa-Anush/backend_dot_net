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
        public decimal NotYetDueCurrentInstallmentBaseAmount { get; set; } // Unpaid base for installments NOT YET DUE

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total_InstallmentBaseArrears { get; set; } // Unpaid base for installments that are past due date

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total_LateInterest { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total_PayableSettlement { get; set; } 

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Paid_AgainstNotYetDueCurrentInstallmentBaseAmount { get; set; } = 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Paid_AgainstTotalArrears { get; set; } = 0m;

        // Total paid against late interest (historical)
        [Column(TypeName = "decimal(18,2)")]
        public decimal Paid_AgainstTotalLateInterest { get; set; } = 0m;
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total_OverpaymentCarriedToNext { get; set; } = 0m;

        [Required(ErrorMessage = "Cashflow ref is required")]
        [MaxLength(100)]
        public string Bnpl_PlanSettlementSummaryRef { get; set; } = string.Empty;

        [EnumDataType(typeof(BNPL_PlanSettlementSummary_StatusEnum))]
        [Column(TypeName = "nvarchar(30)")]
        public BNPL_PlanSettlementSummary_StatusEnum Bnpl_PlanSettlementSummary_Status { get; set; } = BNPL_PlanSettlementSummary_StatusEnum.Active;
        
        public bool IsLatest { get; set; } = true;

        [EnumDataType(typeof(Bnpl_PlanSettlementSummary_PaymentStatusEnum))]
        [Column(TypeName = "nvarchar(30)")]
        public Bnpl_PlanSettlementSummary_PaymentStatusEnum Bnpl_PlanSettlementSummary_PaymentStatus { get; set; } = Bnpl_PlanSettlementSummary_PaymentStatusEnum.Unsettled;

        [ConcurrencyCheck]
        public byte[] RowVersion { get; set; }  = new byte[8]; // for optimistic concurrency.
        
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