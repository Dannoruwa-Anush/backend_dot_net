using WebApplication1.DTOs.ResponseDto.Base;

namespace WebApplication1.DTOs.ResponseDto
{
    public class CustomerResponseDto : BaseResponseDto
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNo { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        // Include simplified info about FK: User
        public required UserResponseDto UserResponseDto { get; set; }
    }
}