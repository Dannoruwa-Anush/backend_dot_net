using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.DTOs.RequestDto.Payment
{
    public class BnplInitialPaymentRequestDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Payment amount is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal InitialPayment { get; set; }

        [Required]
        public int Bnpl_PlanTypeID { get; set; }         

        [Required]          
        public int InstallmentCount { get; set; }     
    }
}