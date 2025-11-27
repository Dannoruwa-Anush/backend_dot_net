using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.Base;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Models
{
    [Index(nameof(Bnpl_PlanID), nameof(InstallmentNo), IsUnique = true)]
    public class BNPL_Installment : BaseModel //(In base model: CreatedAt, UpdatedAt)
    {
        [Key]
        public int InstallmentID { get; set; }

        [Required]
        public int InstallmentNo { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Installment_BaseAmount { get; set; } //This is fixed amount for a Bnpl_Plan

        [Required]
        public DateTime Installment_DueDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OverPaymentCarriedFromPreviousInstallment { get; set; } = 0m;

        //LateInterest = (Installment_BaseAmount - OverPaymentCarriedFromPreviousInstallment) × lateInterestRatePerDay × overdueDays
        [Column(TypeName = "decimal(18,2)")]
        public decimal LateInterest { get; set; } = 0m; //Late interest is charged on the unpaid Installment_BaseAmount

        //TotalDueAmount = (Installment_BaseAmount - OverPaymentCarriedFromPreviousInstallment) + LateInterest
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDueAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid_AgainstBase { get; set; } = 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid_AgainstLateInterest { get; set; } = 0m;

        public DateTime? LastPaymentDate { get; set; }

        public DateTime? RefundDate { get; set; }

        public DateTime? LastLateInterestAppliedDate { get; set; }

        [EnumDataType(typeof(BNPL_Installment_StatusEnum))]
        [Column(TypeName = "nvarchar(30)")]
        public BNPL_Installment_StatusEnum Bnpl_Installment_Status { get; set; } = BNPL_Installment_StatusEnum.Pending;

        [ConcurrencyCheck]
        public byte[] RowVersion { get; set; }  = new byte[8]; // for optimistic concurrency.

        [NotMapped]
        public decimal RemainingBalance => TotalDueAmount - (AmountPaid_AgainstBase + AmountPaid_AgainstLateInterest);
               
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