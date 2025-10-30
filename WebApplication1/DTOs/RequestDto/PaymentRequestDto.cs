using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.RequestDto
{
    public class PaymentRequestDto
    {
        [Required(ErrorMessage = "Amount paid is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        //FK
        public int OrderID { get; set; }
    }
}