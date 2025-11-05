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
            CreateMap<RegisterRequestDto, User>();

            // Entity → Response DTO
            CreateMap<User, LoginResponseDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.Token, opt => opt.Ignore()); // Token is set manually
        }
    }
}
