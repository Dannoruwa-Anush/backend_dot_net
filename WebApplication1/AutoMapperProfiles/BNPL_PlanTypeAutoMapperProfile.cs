using AutoMapper;
using WebApplication1.DTOs.RequestDto;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.Models;

namespace WebApplication1.AutoMapperProfiles
{
    public class BNPL_PlanTypeAutoMapperProfiles : Profile
    {
        public BNPL_PlanTypeAutoMapperProfiles()
        {
            CreateMap<BNPL_PlanType, BNPL_PlanTypeResponseDto>();
            CreateMap<BNPL_PlanTypeRequestDto, BNPL_PlanType>();
        }
    }
}