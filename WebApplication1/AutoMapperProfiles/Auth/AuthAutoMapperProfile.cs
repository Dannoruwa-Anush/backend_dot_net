using AutoMapper;
using WebApplication1.DTOs.RequestDto.Auth;
using WebApplication1.DTOs.ResponseDto.Auth;
using WebApplication1.Models;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.AutoMapperProfiles.Auth
{
    public class AuthAutoMapperProfile : Profile
    {
        public AuthAutoMapperProfile()
        {
            // Request DTO -> Entity (User)
            CreateMap<RegisterRequestDto, User>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Trim()))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password.Trim()));

            // Entity -> Response DTO
            CreateMap<User, LoginResponseDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.CustomerID, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustomerID : (int?)null))
                .ForMember(dest => dest.EmployeePosition, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.Position : (EmployeePositionEnum?)null))
                .ForMember(dest => dest.Token, opt => opt.Ignore()); //Token is set manually in AuthController
        }
    }
}
