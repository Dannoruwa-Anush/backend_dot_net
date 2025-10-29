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
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Item price is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ItemPrice { get; set; }

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