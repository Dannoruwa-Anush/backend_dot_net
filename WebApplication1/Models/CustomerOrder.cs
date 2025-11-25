using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models.Base;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Models
{
    public class CustomerOrder : BaseModel //(In base model: CreatedAt, UpdatedAt)
    {
        [Key]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "Total amount is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Order date is required")]
        public DateTime OrderDate { get; set; }

        public DateTime? ShippedDate { get; set; }

        public DateTime? DeliveredDate { get; set; }

        public DateTime? CancelledDate { get; set; }

        public DateTime? CancellationRequestDate { get; set; }

        [MaxLength(100)]
        public string? CancellationReason { get; set; } 
        
        public bool? CancellationApproved { get; set; } 
        
        [MaxLength(100)]
        public string? CancellationRejectionReason { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        [EnumDataType(typeof(OrderStatusEnum))]
        public OrderStatusEnum OrderStatus { get; set; } = OrderStatusEnum.Pending;

        public DateTime? PaymentCompletedDate { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        [EnumDataType(typeof(OrderPaymentStatusEnum))]
        public OrderPaymentStatusEnum OrderPaymentStatus { get; set; } = OrderPaymentStatusEnum.Pending;

        [ConcurrencyCheck]
        public byte[] RowVersion { get; set; }  = new byte[8]; // for optimistic concurrency.

        //******* [Start: Customer (1) — CustomerOrder (M)] ****
        //FK
        public int CustomerID { get; set; }

        // Many Side: Navigation property
        [ForeignKey(nameof(CustomerID))]
        [InverseProperty(nameof(Customer.CustomerOrders))]
        public required Customer Customer { get; set; }
        //******* [End: Customer (1) — CustomerOrder (M)] ******


        //******* [Start: CustomerOrder (1) — Cashflow (M)] ****
        // One Side: Navigation property
        [InverseProperty(nameof(Cashflow.CustomerOrder))]
        public ICollection<Cashflow> Cashflows { get; set; } = new List<Cashflow>();
        //******* [End: CustomerOrder (1) — Cashflow (M)] ******


        //******* [Start: CustomerOrder (1) — BNPL_PLAN (1)] ****
        // One Side: Navigation property
        [InverseProperty(nameof(BNPL_PLAN.CustomerOrder))]
        public BNPL_PLAN? BNPL_PLAN { get; set; }
        //Nullable (?) : some orders are fully paid upfront, so they have no BNPL plan
        //******* [End: CustomerOrder (1) — BNPL_PLAN (1)] ******

        //******* [Start: CustomerOrderElectronicItem(M) —- CustomerOrder(1)] *******
        // One Side: Navigation property
        [InverseProperty(nameof(CustomerOrderElectronicItem.CustomerOrder))]
        public ICollection<CustomerOrderElectronicItem> CustomerOrderElectronicItems { get; set; } = new List<CustomerOrderElectronicItem>();
        //******* [End: CustomerOrderElectronicItem(M) —- CustomerOrder(1)] *********
    }
}