using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.RequestDto.Auth
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;
    }
}