using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Payment
    {
        [Key]
        public int PaymentID { get; set; }

        public DateTime PaymentDate { get; set; }

        public decimal AmountPaid { get; set; }
        
        //FK: Order

    }
}