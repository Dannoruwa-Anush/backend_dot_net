using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.Base;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Models
{
    public class Employee : BaseModel //(In base model: CreatedAt, UpdatedAt)
    {
        [Key]
        public int EmployeeID { get; set; }

        [Required(ErrorMessage = "Employee name is required")]
        [MaxLength(100)]
        public string EmployeeName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        [EnumDataType(typeof(EmployeePositionEnum))]
        public EmployeePositionEnum Position { get; set; } = EmployeePositionEnum.Cashier;

        //******* [Start: User (1) — Employee (1)] ****
        //FK
        public int UserID { get; set; }

        // One Side: Navigation property
        [ForeignKey(nameof(UserID))]
        [InverseProperty(nameof(User.Employee))]
        public required User User { get; set; }
        //******* [End: User (1) — Employee (1)] ******
    }
}