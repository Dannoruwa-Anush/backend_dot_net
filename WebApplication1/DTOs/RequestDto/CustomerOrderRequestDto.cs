using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs.RequestDto
{
    public class CustomerOrderRequestDto
    {        
        // Adding Electronic Items to the Order
        [Required(ErrorMessage = "At least one electronic item must be added")]
        public List<CustomerOrderElectronicItemRequestDto> CustomerOrderElectronicItems { get; set; } = new List<CustomerOrderElectronicItemRequestDto>();
    
        //FK
        public int CustomerID { get; set; }
    }
}