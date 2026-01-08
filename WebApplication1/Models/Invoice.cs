using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models.Base;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Models
{
    public class Invoice : BaseModel //(In base model: Audit fields)
    {
        [Key]
        public int InvoiceID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal InvoiceAmount { get; set; }

        //--------------------------
        // Invoice Status lifecycle
        //--------------------------
        [Required]
        [Column(TypeName = "nvarchar(20)")]
        [EnumDataType(typeof(InvoiceStatusEnum))]
        public InvoiceStatusEnum InvoiceStatus { get; set; } = InvoiceStatusEnum.Unpaid;
        
        public DateTime? VoidedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? RefundedAt { get; set; }

        // For bnpl installment payment
        public int? InstallmentNo { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        [EnumDataType(typeof(InvoiceTypeEnum))]
        public InvoiceTypeEnum InvoiceType { get; set; } = InvoiceTypeEnum.Bnpl_Initial_Payment;

        [ConcurrencyCheck]
        public byte[] RowVersion { get; set; }  = new byte[8]; // for optimistic concurrency.
        
        //******* [Start: CustomerOrder (1) — Invoice (M)] ****
        //FK
        public int OrderID { get; set; }

        // Many Side: Navigation property
        [ForeignKey(nameof(OrderID))]
        [InverseProperty(nameof(CustomerOrder.Invoices))]
        public CustomerOrder? CustomerOrder { get; set; }
        //******* [End: CustomerOrder (1) — Invoice (M)] ******


        //******* [Start: Invoice (1) — Cashflow (0..1)] ****
        // One Side: Navigation property
        [InverseProperty(nameof(Cashflow.Invoice))]
        public Cashflow? Cashflow { get; set; }
        //Nullable (?) : Only paid Invoice will be create a cashflow
        //******* [End: Invoice (1) — Cashflow (0..1)] ******
    }
}