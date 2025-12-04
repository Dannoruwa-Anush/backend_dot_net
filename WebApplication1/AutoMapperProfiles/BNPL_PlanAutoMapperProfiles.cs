using AutoMapper;
using WebApplication1.DTOs.RequestDto;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.Models;

namespace WebApplication1.AutoMapperProfiles
{
    public class BNPL_PlanAutoMapperProfiles : Profile
    {
        public BNPL_PlanAutoMapperProfiles()
        {
            CreateMap<BNPL_PLAN, BNPL_PlanResponseDto>()
                .ForMember(dest => dest.BNPL_PlanTypeResponseDto, opt => opt.MapFrom(src => src.BNPL_PlanType))
                .ForMember(dest => dest.CustomerOrderResponseDto, opt => opt.MapFrom(src => src.CustomerOrder));
        }
    }
}