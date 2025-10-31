using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class BNPL_PlanType
    {
        [Key]
        public int Bnpl_PlanTypeID { get; set; }

        [Required(ErrorMessage = "Bnpl plan type name is required")]
        [MaxLength(100)]
        public string Bnpl_PlanTypeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bnpl duration is required")]
        public int Bnpl_DurationDays { get; set; }

        [Required(ErrorMessage = "Interest rate is required")]
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100, ErrorMessage = "Interest rate must be between 0 and 100")]
        public decimal InterestRate { get; set; }

        [Required(ErrorMessage = "Late payment interest rate is required")]
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100, ErrorMessage = "Late payment interest rate must be between 0 and 100")]
        public decimal LatePayInterestRate { get; set; }

        public string Bnpl_Description { get; set; } = string.Empty;

        //for: creation/modification tracking
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        //******* [Start: BNPL_PlanType (1) — BNPL_PLAN (M)] ****
        // One Side: Navigation property
        [InverseProperty(nameof(BNPL_PLAN.BNPL_PlanType))]
        public ICollection<BNPL_PLAN> BNPL_PLANs { get; set; } = new List<BNPL_PLAN>();
        //******* [End: BNPL_PlanType (1) — BNPL_PLAN (M)] ******
    }
}