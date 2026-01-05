using AutoMapper;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.Models;

namespace WebApplication1.AutoMapperProfiles
{
    public class UserAutoMappingProfile : Profile
    {
        public UserAutoMappingProfile()
        {
            CreateMap<User, UserResponseDto>();
        }
    }
}