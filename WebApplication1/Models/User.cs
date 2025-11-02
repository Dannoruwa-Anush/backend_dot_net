using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.Base;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class User : BaseModel //(In base model: CreatedAt, UpdatedAt)
    {
        [Key]
        public int UserID { get; set; }

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

        //******* [Start: User (1) — Employee (1)] ****
        // One Side: Navigation property
        [InverseProperty(nameof(Employee.User))]
        public Employee? Employee { get; set; }
        //******* [End: User (1) — Employee (1)] ******

        //******* [Start: User (1) — Customer (1)] ****
        [InverseProperty(nameof(Customer.User))]
        public Customer? Customer { get; set; }
        //******* [End: User (1) — Customer (1)] ******
    }
}