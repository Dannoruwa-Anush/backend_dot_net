using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class Customer
    {
        [Key]
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "Customer name is required")]
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [MaxLength(15)]
        public string PhoneNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [MaxLength(255)]
        public string Address { get; set; } = string.Empty;

        //******* [Start: Customer (1) — CustomerOrder (M)] ****
        // One Side: Navigation property
        public ICollection<CustomerOrder> CustomerOrders { get; set; } = new List<CustomerOrder>();
        //******* [End: Customer (1) — CustomerOrder (M)] ******
    }
}