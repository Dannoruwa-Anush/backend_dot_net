using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.RequestDto.UserProfileUpdate
{
    public class EmployeeProfileUpdateRequestDto
    {        
        [Required(ErrorMessage = "Employee name is required")]
        [MaxLength(100)]
        public string EmployeeName { get; set; } = string.Empty;
    }
}