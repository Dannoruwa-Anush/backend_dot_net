using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.RequestDto
{
    public class BNPL_PlanTypeRequestDto
    {
        [Required(ErrorMessage = "Bnpl plan type name is required")]
        [MaxLength(100)]
        public string Bnpl_PlanTypeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bnpl duration is required")]
        public int Bnpl_DurationDays { get; set; }

        public string Bnpl_Description { get; set; } = string.Empty;
    }
}