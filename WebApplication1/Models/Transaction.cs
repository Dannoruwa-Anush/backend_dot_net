using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionID { get; set; }

        [Required(ErrorMessage = "Amount paid is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        [Required(ErrorMessage = "Transaction ref is required")]
        [MaxLength(100)]
        public string TransactionRef { get; set; } = string.Empty;

        [Required(ErrorMessage = "Transaction date time is required")]
        public DateTime TransactionDate { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        [EnumDataType(typeof(TransactionStatusEnum))]
        public TransactionStatusEnum TransactionStatus { get; set; } = TransactionStatusEnum.Paid;

        public DateTime? RefundDate { get; set; }

        //for: creation/modification tracking
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        //******* [Start: CustomerOrder (1) — Transaction (M)] ****
        //FK
        public int OrderID { get; set; }

        // Many Side: Navigation property
        [ForeignKey(nameof(OrderID))]
        [InverseProperty(nameof(CustomerOrder.Transactions))]
        public required CustomerOrder CustomerOrder { get; set; }
        //******* [End: CustomerOrder (1) — Transaction (M)] ******

    }
}