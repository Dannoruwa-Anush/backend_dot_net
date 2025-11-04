using AutoMapper;
using WebApplication1.DTOs.RequestDto.Auth;
using WebApplication1.DTOs.ResponseDto.Auth;
using WebApplication1.Models;

namespace WebApplication1.AutoMapperProfiles.Auth
{
    public class AuthAutoMapperProfile : Profile
    {
        public AuthAutoMapperProfile()
        {
            // Request DTO → Entity (User)
            CreateMap<RegisterRequestDto, User>()
                .ForMember(dest => dest.Password, opt => opt.Ignore()); // hashed in service layer

            // Entity → Response DTO
            CreateMap<User, LoginResponseDto>()
                .ForMember(dest => dest.Token, opt => opt.Ignore()) // added later in controller
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));
        }
    }
}
