using AutoMapper;
using WebApplication1.DTOs.RequestDto;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.Models;

namespace WebApplication1.AutoMapperProfiles
{
    public class CustomerAutoMapperProfiles : Profile
    {
        public CustomerAutoMapperProfiles()
        {
            CreateMap<Customer, CustomerResponseDto>()
                    .ForMember(dest => dest.UserResponseDto, opt => opt.MapFrom(src => src.User));
                    
            CreateMap<CustomerRequestDto, Customer>();
        }
    }
}