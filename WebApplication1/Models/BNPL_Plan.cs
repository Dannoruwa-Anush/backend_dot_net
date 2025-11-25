using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models.Base;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Models
{
    public class BNPL_PLAN : BaseModel //(In base model: CreatedAt, UpdatedAt)
    {
        [Key]
        public int Bnpl_PlanID { get; set; }

        [Required(ErrorMessage = "Initial payment is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Bnpl_InitialPayment { get; set; }

        [Required(ErrorMessage = "Amount per installment is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Bnpl_AmountPerInstallment { get; set; }

        [Required(ErrorMessage = "Total installment count is required")]
        public int Bnpl_TotalInstallmentCount { get; set; }

        [Required(ErrorMessage = "Remaining installment count is required")]
        public int Bnpl_RemainingInstallmentCount { get; set; }

        [Required(ErrorMessage = "Bnpl Start date is required")]
        public DateTime Bnpl_StartDate { get; set; }

        public DateTime? Bnpl_NextDueDate { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        [EnumDataType(typeof(BnplStatusEnum))]
        public BnplStatusEnum Bnpl_Status { get; set; } = BnplStatusEnum.Active;

        [ConcurrencyCheck]
        public byte[] RowVersion { get; set; }  = new byte[8]; // for optimistic concurrency.

        //******* [Start: BNPL_PlanType (1) — BNPL_PLAN (M)] ****
        //FK
        public int Bnpl_PlanTypeID { get; set; }

        // Many Side: Navigation property
        [ForeignKey(nameof(Bnpl_PlanTypeID))]
        public BNPL_PlanType? BNPL_PlanType { get; set; }
        //******* [End: BNPL_PlanType (1) — BNPL_PLAN (M)] ******


        //******* [Start: CustomerOrder (1) — BNPL_PLAN (1)] ****
        //FK
        public int OrderID { get; set; }

        // One Side: Navigation property
        [ForeignKey(nameof(OrderID))]
        [InverseProperty(nameof(CustomerOrder.BNPL_PLAN))]
        public CustomerOrder? CustomerOrder { get; set; }
        //******* [End: CustomerOrder (1) — BNPL_PLAN (1)] ******


        //******* [Start: BNPL_PLAN (1) — BNPL_Installment (M)] ****
        // One Side: Navigation property
        [InverseProperty(nameof(BNPL_Installment.BNPL_PLAN))]
        public ICollection<BNPL_Installment> BNPL_Installments { get; set; } = new List<BNPL_Installment>();
        //******* [End: BNPL_PLAN (1) — BNPL_Installment (M)] ******

        //******* [Start: BNPL_PLAN (1) — BNPL_PlanSettlementSummary (M)] ****
        // One Side: Navigation property
        [InverseProperty(nameof(BNPL_PlanSettlementSummary.BNPL_PLAN))]
        public ICollection<BNPL_PlanSettlementSummary> BNPL_PlanSettlementSummaries { get; set; } = new List<BNPL_PlanSettlementSummary>();
        //******* [End: BNPL_PLAN (1) — BNPL_PlanSettlementSummary (M)] ******
    }
}