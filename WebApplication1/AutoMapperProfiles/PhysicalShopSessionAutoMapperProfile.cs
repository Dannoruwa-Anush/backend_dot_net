using AutoMapper;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.Models;

namespace WebApplication1.AutoMapperProfiles
{
    public class PhysicalShopSessionAutoMapperProfile : Profile
    {
        public PhysicalShopSessionAutoMapperProfile()
        {
            CreateMap<PhysicalShopSession, PhysicalShopSessionResponseDto>();
            //No Request : because creation will be handled by backend
        }
    }
}