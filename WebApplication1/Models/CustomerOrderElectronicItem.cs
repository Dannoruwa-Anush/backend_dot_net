using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class CustomerOrderElectronicItem
    {
        //This is joint table (CustomerOrder(M) - ElectronicItem(M))

        [Key]
        public int OrderItemID { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Item price is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ItemPrice { get; set; }

        //******* [Start: CustomerOrderElectronicItem (1) — ElectronicItem (1)] ******
        //FK
        public int E_ItemID { get; set; }

        // One Side: Navigation property
        [ForeignKey(nameof(E_ItemID))]
        public required ElectronicItem ElectronicItem { get; set; }
        //******* [End: CustomerOrderElectronicItem (1) — ElectronicItem (1)] ********


        //******* [Start: CustomerOrderElectronicItem (1) — CustomerOrder (1)] *******
        //FK
        public int OrderID { get; set; }

        // One Side: Navigation property
        [ForeignKey(nameof(OrderID))]
        public required CustomerOrder CustomerOrder { get; set; }
        //******* [End: CustomerOrderElectronicItem (1) — CustomerOrder (1)] *********
    }
}