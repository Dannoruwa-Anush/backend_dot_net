using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.DTOs.RequestDto.Payment
{
    public class PaymentRequestDto
    {
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Payment amount is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PaymentAmount { get; set; } //bnpl case: = InitialPayment

        //Only for bnpl initial payment        
        public int? Bnpl_PlanTypeID { get; set; }                   
        public int? InstallmentCount { get; set; }     
    }
}