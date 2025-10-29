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
            CreateMap<BNPL_PLAN, BNPL_PlanResponseDto>();
            CreateMap<BNPL_PlanRequestDto, BNPL_PLAN>();
        }
    }
}