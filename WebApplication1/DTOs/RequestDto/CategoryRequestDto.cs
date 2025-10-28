using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.RequestDto
{
    public class CategoryRequestDto
    {
        [Required(ErrorMessage = "Category name is required")]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;
    }
}