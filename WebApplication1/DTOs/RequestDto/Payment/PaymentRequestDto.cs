using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.RequestDto.Payment
{
    public class PaymentRequestDto
    {
        [Required]
        public int InvoiceId { get; set; }
    }
}