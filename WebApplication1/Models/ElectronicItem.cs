using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class ElectronicItem
    {
        [Key]
        public int E_ItemID { get; set; }

        public string E_ItemName { get; set; } = string.Empty;

        public decimal Price { get; set; } 
        
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
    }
}