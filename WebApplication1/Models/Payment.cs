using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Payment
    {
        [Key]
        public int PaymentID { get; set; }

        [Required(ErrorMessage = "Payment date time is required")]
        public DateTime PaymentDate { get; set; }

        [Required(ErrorMessage = "Amount paid is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        //for: creation/modification tracking
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        //******* [Start: CustomerOrder (1) — Payment (M)] ****
        //FK
        public int OrderID { get; set; }

        // Many Side: Navigation property
        [ForeignKey(nameof(OrderID))]
        public required CustomerOrder CustomerOrder { get; set; }
        //******* [End: CustomerOrder (1) — Payment (M)] ******

    }
}