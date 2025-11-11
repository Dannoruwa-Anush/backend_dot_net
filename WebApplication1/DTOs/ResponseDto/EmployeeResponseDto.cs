using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.ResponseDto
{
    public class EmployeeResponseDto
    {
        public int EmployeeID { get; set; }

        public string EmployeeName { get; set; } = string.Empty;

        public EmployeePositionEnum Position { get; set; } = EmployeePositionEnum.Cashier;

        // Include simplified info about FK: User
        public required UserResponseDto UserResponseDto { get; set; }
    }
}