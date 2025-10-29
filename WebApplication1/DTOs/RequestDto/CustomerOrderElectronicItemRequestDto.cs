using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.DTOs.RequestDto
{
    public class CustomerOrderElectronicItemRequestDto
    {
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Item price is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ItemPrice { get; set; }

        //FK
        public int E_ItemID { get; set; }
    }
}