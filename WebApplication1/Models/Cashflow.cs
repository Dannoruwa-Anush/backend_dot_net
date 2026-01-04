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

        [Required(ErrorMessage = "Amount paid is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        [Required(ErrorMessage = "Cashflow ref is required")]
        [MaxLength(100)]
        public string CashflowRef { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cashflow date time is required")]
        public DateTime CashflowDate { get; set; }

        public DateTime? RefundDate { get; set; }
        
        [Required]
        [Column(TypeName = "nvarchar(20)")]
        [EnumDataType(typeof(CashflowStatusEnum))]
        public CashflowStatusEnum CashflowStatus { get; set; } = CashflowStatusEnum.Paid;
        
        [ConcurrencyCheck]
        public byte[] RowVersion { get; set; }  = new byte[8]; // for optimistic concurrency.

        //******* [Start: Invoice (1) — Cashflow (0..1)] ****
        //FK
        public int InvoiceID { get; set; }

        // One Side: Navigation property
        [ForeignKey(nameof(InvoiceID))]
        [InverseProperty(nameof(Invoice.Cashflow))]
        //public required Invoice Invoice { get; set; }
        public Invoice? Invoice { get; set; } ///testing
        //******* [Start: Invoice (1) — Cashflow (0..1)] ****
    }
}