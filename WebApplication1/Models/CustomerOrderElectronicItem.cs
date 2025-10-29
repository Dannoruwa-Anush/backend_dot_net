using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class CustomerOrderElectronicItem
    {
        //This is joint table (CustomerOrder(M) - ElectronicItem(M))

        [Key]
        public int OrderItemID { get; set; }

        public int Quantity { get; set; } 

        public decimal ItemPrice { get; set; }

        //Fk: CustomerOrder 
        //Fk: ElectronicItem 
    }
}