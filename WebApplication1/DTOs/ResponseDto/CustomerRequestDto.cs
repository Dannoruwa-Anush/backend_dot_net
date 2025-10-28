namespace WebApplication1.DTOs.ResponseDto
{
    public class CustomerResponseDto
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNo { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}