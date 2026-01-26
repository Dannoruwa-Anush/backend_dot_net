using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.DTOs.RequestDto.Payment
{
    public class PaymentRequestDto
    {
        [Required]
        public int InvoiceId { get; set; }
    }
}