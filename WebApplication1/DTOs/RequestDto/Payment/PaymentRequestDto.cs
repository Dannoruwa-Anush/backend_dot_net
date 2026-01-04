using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.DTOs.RequestDto.Payment
{
    public class PaymentRequestDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        [Required(ErrorMessage = "Payment amount is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PaymentAmount { get; set; } 
    }
}