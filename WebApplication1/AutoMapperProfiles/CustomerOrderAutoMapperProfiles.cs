using AutoMapper;
using WebApplication1.DTOs.RequestDto;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.Models;

namespace WebApplication1.AutoMapperProfiles
{
    public class CustomerOrderAutoMapperProfiles : Profile
    {
        public CustomerOrderAutoMapperProfiles()
        {
            CreateMap<CustomerOrder, CustomerOrderResponseDto>();
            CreateMap<CustomerOrderRequestDto, CustomerOrder>();
        }
    }
}