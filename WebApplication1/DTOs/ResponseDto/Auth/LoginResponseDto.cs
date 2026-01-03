using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.ResponseDto.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRoleEnum Role { get; set; } = UserRoleEnum.Customer;
        public int UserID { get; set; }

        //If user is a customer
        public int? CustomerID { get; set; }
        // If user is an employee
        public EmployeePositionEnum? EmployeePosition { get; set; }
    }
}