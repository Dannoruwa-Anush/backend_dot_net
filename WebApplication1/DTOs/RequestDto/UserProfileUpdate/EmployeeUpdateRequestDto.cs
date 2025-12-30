using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.RequestDto.UserProfileUpdate
{
    public class EmployeeUpdateRequestDto
    {        
        [Required(ErrorMessage = "Employee name is required")]
        [MaxLength(100)]
        public string EmployeeName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        [EnumDataType(typeof(EmployeePositionEnum))]
        public EmployeePositionEnum Position { get; set; } = EmployeePositionEnum.Cashier;
    }
}