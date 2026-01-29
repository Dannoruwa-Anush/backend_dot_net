using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models.Base;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Models
{
    public class Cashflow : BaseModel //(In base model: Audit fields)
    {
        [Key]
        public int CashflowID { get; set; }

        // + = money in, - = money out
        [Required(ErrorMessage = "Amount paid is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        [Required(ErrorMessage = "Cashflow ref is required")]
        [MaxLength(100)]
        public string CashflowRef { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cashflow date time is required")]
        public DateTime CashflowDate { get; set; }
        
        [Required]
        [Column(TypeName = "nvarchar(20)")]
        [EnumDataType(typeof(CashflowPaymentNatureEnum))]
        public CashflowPaymentNatureEnum CashflowPaymentNature { get; set; } = CashflowPaymentNatureEnum.Payment;
        
        // Receipt = proof of payment (Generated after payment)
        [Column(TypeName = "nvarchar(255)")]
        public string? PaymentReceiptFileUrl { get; set; }   // Payment receipt

        // RefundReceiptFil = proof of refund payment (Generated after order cancel)
        public string? RefundReceiptFileUrl { get; set; } // Refund receipt

        [ConcurrencyCheck]
        public byte[] RowVersion { get; set; }  = new byte[8]; // for optimistic concurrency.

        //******* [Start: Invoice (1) — Cashflow (M: payment, refund)] ****
        //FK
        public int InvoiceID { get; set; }

        // Many Side: Navigation property
        [ForeignKey(nameof(InvoiceID))]
        public Invoice? Invoice { get; set; }
        //******* [Start: Invoice (1) — Cashflow (M)] ****
    }
}