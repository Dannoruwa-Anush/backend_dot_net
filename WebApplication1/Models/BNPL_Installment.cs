using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models.Base;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Models
{
    public class BNPL_Installment : BaseModel //(In base model: CreatedAt, UpdatedAt)
    {
        [Key]
        public int InstallmentID { get; set; }

        [Required]
        public int InstallmentNo { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Installment_BaseAmount { get; set; }

        [Required]
        public DateTime Installment_DueDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OverPaymentCarried { get; set; } = 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal ArrearsCarried { get; set; } = 0m;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal LateInterest { get; set; } = 0m;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDueAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; } = 0m;
        public DateTime? LastPaymentDate { get; set; }
        public DateTime? RefundDate { get; set; }

        [EnumDataType(typeof(BNPL_Installment_StatusEnum))]
        [Column(TypeName = "nvarchar(30)")]
        public BNPL_Installment_StatusEnum Bnpl_Installment_Status { get; set; } = BNPL_Installment_StatusEnum.Pending;

        // Derived convenience fields
        [NotMapped]
        public decimal RemainingBalance => TotalDueAmount - AmountPaid;

        [NotMapped]
        public bool IsOverdue => Installment_DueDate < DateTime.UtcNow && Bnpl_Installment_Status == BNPL_Installment_StatusEnum.Pending;

        //******* [Start: BNPL_PLAN (1) — BNPL_Installment (M)] ****
        //FK
        public int Bnpl_PlanID { get; set; }

        // Many Side: Navigation property
        [ForeignKey(nameof(Bnpl_PlanID))]
        [InverseProperty(nameof(BNPL_PLAN.BNPL_Installments))]
        public BNPL_PLAN? BNPL_PLAN { get; set; }
        //******* [End: BNPL_PLAN (1) — BNPL_Installment (M)] ******
    }
}