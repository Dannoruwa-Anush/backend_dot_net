using AutoMapper;
using WebApplication1.Models;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.DTOs.RequestDto;

namespace WebApplication1.AutoMapperProfiles
{
    public class BrandAutoMapperProfile : Profile
    {
        public BrandAutoMapperProfile()
        {
            CreateMap<Brand, BrandResponseDto>();
            CreateMap<BrandRequestDto, Brand>();
        }
    }
}
