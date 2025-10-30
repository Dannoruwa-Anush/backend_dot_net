using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.DTOs.RequestDto
{
    public class CustomerOrderElectronicItemRequestDto
    {
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Unit price is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Sub total is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        //FK
        public int E_ItemID { get; set; }
    }
}