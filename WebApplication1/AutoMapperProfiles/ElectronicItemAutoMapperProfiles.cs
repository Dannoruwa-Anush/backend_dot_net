using AutoMapper;
using WebApplication1.DTOs.RequestDto;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.Models;

namespace WebApplication1.AutoMapperProfiles
{
    public class ElectronicItemAutoMapperProfiles : Profile
    {
        public ElectronicItemAutoMapperProfiles()
        {
            CreateMap<ElectronicItem, ElectronicItemResponseDto>()
                .ForMember(dest => dest.BrandResponseDto, opt => opt.MapFrom(src => src.Brand))
                .ForMember(dest => dest.CategoryResponseDto, opt => opt.MapFrom(src => src.Category));

            CreateMap<ElectronicItemRequestDto, ElectronicItem>();
        }
    }
}