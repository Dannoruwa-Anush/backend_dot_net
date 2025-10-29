using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.RequestDto
{
    public class CustomerRequestDto
    {
        [Required(ErrorMessage = "Customer name is required")]
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [MaxLength(15)]
        [RegularExpression(@"^\+?[0-9]{10,15}$", ErrorMessage = "Invalid phone number format. Use digits only, optionally starting with +, and length between 10-15.")]
        public string PhoneNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [MaxLength(255)]
        public string Address { get; set; } = string.Empty;
    }
}