using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models.Base;
using WebApplication1.Utils.Project_Enums;

/*
1. [frontend] Add Products to shopping cart (cart-unlock: Add/change products, qty)
2. [frontend] Checkout order (cart-lock: products in shopping cart)
2. [frontend] Choose order mode (Pay_Now, Bnpl)
3. [frontend] Review (check order details [cancel/confirm])
4. [backendend/create table record] After Order Confirmation: order, invoice (Unpaid) bnpl plan, bnpl_installmets, bnpl_snapshot creates and waiting for payment
5. [backendend/update record status] Process Payment: 
                5.1 if payment received: invoice (paid), 
                5.2 if order is cancelled before the payment: invoice (Voided), 
                5.3 if order is refund_approved after the payment: invoice (Refunded), 
*/
namespace WebApplication1.Models
{
    public class CustomerOrder : BaseModel //(In base model: Audit fields)
    {
        //--------------------------
        // Basic Info
        //--------------------------
        [Key]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "Total amount is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Order date is required")]
        public DateTime OrderDate { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        [EnumDataType(typeof(OrderSourceEnum))]
        public OrderSourceEnum OrderSource { get; set; } = OrderSourceEnum.PhysicalShop;

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        [EnumDataType(typeof(OrderPaymentModeEnum))]
        public OrderPaymentModeEnum OrderPaymentMode { get; set; } = OrderPaymentModeEnum.Pay_Bnpl;

        //--------------------------
        // Order Status lifecycle
        //--------------------------
        [Required]
        [Column(TypeName = "nvarchar(20)")]
        [EnumDataType(typeof(OrderStatusEnum))]
        public OrderStatusEnum OrderStatus { get; set; } = OrderStatusEnum.Pending;

        public DateTime? ShippedDate { get; set; }

        public DateTime? DeliveredDate { get; set; }

        public DateTime? CancelledDate { get; set; }

        public DateTime? CancellationRequestDate { get; set; }

        [MaxLength(100)]
        public string? CancellationReason { get; set; }

        public bool? CancellationApproved { get; set; }

        [MaxLength(100)]
        public string? CancellationRejectionReason { get; set; }

        //--------------------------
        // Payment Status lifecycle
        //--------------------------
        [Required]
        [Column(TypeName = "nvarchar(20)")]
        [EnumDataType(typeof(OrderPaymentStatusEnum))]
        public OrderPaymentStatusEnum OrderPaymentStatus { get; set; } = OrderPaymentStatusEnum.Awaiting_Payment;

        // only for online : now + 5 min.
        public DateTime? PendingPaymentOrderAutoCancelledDate { get; set; }
        public DateTime? PaymentCompletedDate { get; set; }

        [ConcurrencyCheck]
        public byte[] RowVersion { get; set; } = new byte[8]; // for optimistic concurrency.

        //******* [Start: CustomerOrder (M) - PhysicalShopSession (0..1)] ****
        //FK
        // PhysicalShopSessionId is nullable to online orders
        public int? PhysicalShopSessionId { get; set; }

        // Many Side: Navigation property
        [ForeignKey(nameof(PhysicalShopSessionId))]
        [InverseProperty(nameof(PhysicalShopSession.CustomerOrders))]
        public PhysicalShopSession? PhysicalShopSession { get; set; } //Nullable navigation property to allow online orders
        //******* [End: CustomerOrder (M) - PhysicalShopSession (0..1)] ******


        //******* [Start: Customer (0..1) — CustomerOrder (M)] ****
        //FK
        // CustomerID is nullable to support cashier's direct orders
        public int? CustomerID { get; set; }

        // Many Side: Navigation property
        [ForeignKey(nameof(CustomerID))]
        [InverseProperty(nameof(Customer.CustomerOrders))]
        public Customer? Customer { get; set; } //Nullable navigation property to allow cashier's direct orders
        //******* [End: Customer (0..1) — CustomerOrder (M)] ******


        //******* [Start: CustomerOrder (1) — Invoice (M)] ****
        // One Side: Navigation property
        [InverseProperty(nameof(Invoice.CustomerOrder))]
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        //******* [End: CustomerOrder (1) — Invoice (M)] ******


        //******* [Start: CustomerOrder (1) — BNPL_PLAN (0..1)] ****
        // One Side: Navigation property
        [InverseProperty(nameof(BNPL_PLAN.CustomerOrder))]
        public BNPL_PLAN? BNPL_PLAN { get; set; }
        //Nullable (?) : some orders are fully paid upfront, so they have no BNPL plan
        //******* [End: CustomerOrder (1) — BNPL_PLAN (0..1)] ******


        //******* [Start: CustomerOrderElectronicItem(M) —- CustomerOrder(1)] *******
        // One Side: Navigation property
        [InverseProperty(nameof(CustomerOrderElectronicItem.CustomerOrder))]
        public ICollection<CustomerOrderElectronicItem> CustomerOrderElectronicItems { get; set; } = new List<CustomerOrderElectronicItem>();
        //******* [End: CustomerOrderElectronicItem(M) —- CustomerOrder(1)] *********
    }
}