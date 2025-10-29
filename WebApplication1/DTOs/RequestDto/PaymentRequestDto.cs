using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.DTOs.RequestDto
{
    public class PaymentRequestDto
    {
        [Required(ErrorMessage = "Payment date time is required")]
        public DateTime PaymentDate { get; set; }

        [Required(ErrorMessage = "Amount paid is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }
        
        //FK
        public int OrderID { get; set; }
    }
}