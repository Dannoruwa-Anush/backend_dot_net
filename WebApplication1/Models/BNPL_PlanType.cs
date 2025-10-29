using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class BNPL_PlanType
    {
        [Key]
        public int Bnpl_PlanTypeID { get; set; }

        [Required(ErrorMessage = "Bnpl plan type name is required")]
        [MaxLength(100)]
        public string Bnpl_PlanTypeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bnpl duration us required")]
        public int Bnpl_DurationDays { get; set; }

        public string Bnpl_Description { get; set; } = string.Empty;

        //******* [Start: BNPL_PlanType (1) — BNPL_PLAN (M)] ****
        // One Side: Navigation property
        public ICollection<BNPL_PLAN> BNPL_PLANs { get; set; } = new List<BNPL_PLAN>();
        //******* [End: BNPL_PlanType (1) — BNPL_PLAN (M)] ******
    }
}