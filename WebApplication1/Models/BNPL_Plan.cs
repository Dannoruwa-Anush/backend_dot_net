using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Models
{
    public class BNPL_PLAN
    {
        [Key]
        public int Bnpl_PlanID { get; set; }

        [Required(ErrorMessage = "Total installments are required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Bnpl_TotalInstallments { get; set; }

        [Required(ErrorMessage = "Installment amount is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Bnpl_InstallmentAmount { get; set; }

        [Required(ErrorMessage = "Interest rate is required")]
        public double Bnpl_InterestRate { get; set; }

        public DateTime Bnpl_StartDate { get; set; }

        public DateTime Bnpl_NextDueDate { get; set; }

        [Required(ErrorMessage = "Remaining balance is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Bnpl_RemainingBalance { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        [EnumDataType(typeof(BnplStatusEnum))]
        public BnplStatusEnum Bnpl_Status { get; set; } = BnplStatusEnum.Incomplete;

        //for: creation/modification tracking
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        //******* [Start: BNPL_PlanType (1) — BNPL_PLAN (M)] ****
        //FK
        public int Bnpl_PlanTypeID { get; set; }

        // Many Side: Navigation property
        [ForeignKey(nameof(Bnpl_PlanTypeID))]
        public required BNPL_PlanType BNPL_PlanType { get; set; }
        //******* [End: BNPL_PlanType (1) — BNPL_PLAN (M)] ******
    }
}