using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Models
{
    public class CustomerOrder
    {
        [Key]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "Order date is required")]
        public DateTime OrderDate { get; set; }

        [Required(ErrorMessage = "Total amount is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        [EnumDataType(typeof(PaymentStatusEnum))]
        public PaymentStatusEnum PaymentStatus { get; set; } = PaymentStatusEnum.Partially_Paid;

        //for: creation/modification tracking
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        //******* [Start: Customer (1) — CustomerOrder (M)] ****
        //FK
        public int CustomerID { get; set; }

        // Many Side: Navigation property
        [ForeignKey(nameof(CustomerID))]
        public required Customer Customer { get; set; }
        //******* [End: Customer (1) — CustomerOrder (M)] ******


        //******* [Start: CustomerOrder (1) — Payment (M)] ****
        // One Side: Navigation property
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        //******* [End: CustomerOrder (1) — Payment (M)] ******
    }
}