using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models.Base;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Models
{
    public class Cashflow : BaseModel //(In base model: CreatedAt, UpdatedAt)
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
        
        //******* [Start: CustomerOrder (1) — Cashflow (M)] ****
        //FK
        public int OrderID { get; set; }

        // Many Side: Navigation property
        [ForeignKey(nameof(OrderID))]
        [InverseProperty(nameof(CustomerOrder.Cashflows))]
        public CustomerOrder? CustomerOrder { get; set; }
        //******* [End: CustomerOrder (1) — Cashflow (M)] ******

    }
}