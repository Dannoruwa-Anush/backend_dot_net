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
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } 
        
        [Required(ErrorMessage = "QOH is required")]
        public int QOH { get; set; }
        
        //FK
        public int BrandId { get; set; }

        //FK
        public int CategoryID { get; set; }
    }
}