using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.DTOs.RequestDto
{
    public class ElectronicItemRequestDto
    {
        [Required(ErrorMessage = "Item name is required")]
        [MaxLength(100)]
        public string E_ItemName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "QOH is required")]
        [Range(0, int.MaxValue, ErrorMessage = "QOH cannot be negative")]
        public int QOH { get; set; }

        // Image upload
        public IFormFile? ImageFile { get; set; }
        
        //FK
        public int BrandID { get; set; }

        //FK
        public int CategoryID { get; set; }
    }
}