using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.RequestDto
{
    public class BrandRequestDto
    {
        [Required(ErrorMessage = "Brand name is required")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}