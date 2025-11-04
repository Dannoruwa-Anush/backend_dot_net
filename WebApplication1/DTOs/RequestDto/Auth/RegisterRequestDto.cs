using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.RequestDto.Auth
{
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        [EnumDataType(typeof(UserRoleEnum))]
        public UserRoleEnum Role { get; set; } = UserRoleEnum.Customer; 
    }
}