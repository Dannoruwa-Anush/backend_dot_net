using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models.Base;

namespace WebApplication1.Models
{
    public class CustomerOrderElectronicItem : BaseModel //(In base model: Audit fields)
    {
        //This is joint table (CustomerOrder(M) - ElectronicItem(M))

        //Composite Key : order_id + E_ItemID  (will be handled in AppDbContext.cs)

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Unit price is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Sub total is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [ConcurrencyCheck]
        public byte[] RowVersion { get; set; }  = new byte[8]; // for optimistic concurrency.

        //******* [Start: ElectronicItems (1) — CustomerOrderElectronicItem(M)] ******
        //FK
        public int E_ItemID { get; set; }

        // Many Side: Navigation property
        [ForeignKey(nameof(E_ItemID))]
        public required ElectronicItem ElectronicItem { get; set; }
        //******* [End: ElectronicItems (1) — CustomerOrderElectronicItem(M)] ********


        //******* [Start: CustomerOrderElectronicItem(M) —- CustomerOrder(1)] *******
        //FK
        public int OrderID { get; set; }

        // Many Side: Navigation property
        [ForeignKey(nameof(OrderID))]
        public required CustomerOrder CustomerOrder { get; set; }
        //******* [End: CustomerOrderElectronicItem(M) —- CustomerOrder(1)] *********
    }
}