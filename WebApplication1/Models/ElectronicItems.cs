using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class ElectronicItems
    {
        [Key]
        public int E_ItemID { get; set; }

        public string E_ItemName { get; set; } = string.Empty;

        public decimal Price { get; set; } 
        
        public int QOH { get; set; }
        
        //Fk: Brand
        //Fk: Category
    }
}