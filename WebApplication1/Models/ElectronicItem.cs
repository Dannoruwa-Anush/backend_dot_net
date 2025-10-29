using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class ElectronicItem
    {
        [Key]
        public int E_ItemID { get; set; }

        [Required(ErrorMessage = "Item name is required")]
        [MaxLength(100)]
        public string E_ItemName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } 
        
        [Required(ErrorMessage = "QOH is required")]
        public int QOH { get; set; }
        
        //******* [Start: Brand (1) — ElectronicItems (M)] ****
        //FK
        public int BrandId { get; set; }

        // Many Side: Navigation property
        [ForeignKey(nameof(BrandId))]
        public required Brand Brand { get; set; }
        //******* [End: Brand (1) — ElectronicItems (M)] ********


        //******* [Start: Category (1) — ElectronicItems (M)] ****
        //FK
        public int CategoryID { get; set; }

        // Many Side: Navigation property
        [ForeignKey(nameof(CategoryID))]
        public required Category Category { get; set; }
        //******* [End: Category (1) — ElectronicItems (M)] ******


        //******* [Start: CustomerOrderElectronicItem (1) — ElectronicItem (1)] ******
        // One Side: Navigation property
        public required CustomerOrderElectronicItem CustomerOrderElectronicItem { get; set; }
        //******* [End: CustomerOrderElectronicItem (1) — ElectronicItem (1)] ********
    }
}