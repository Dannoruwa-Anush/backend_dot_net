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
    }
}