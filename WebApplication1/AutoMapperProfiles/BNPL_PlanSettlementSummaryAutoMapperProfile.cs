using AutoMapper;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.Models;

namespace WebApplication1.AutoMapperProfiles
{
    public class BNPL_PlanSettlementSummaryAutoMapperProfile : Profile
    {
        public BNPL_PlanSettlementSummaryAutoMapperProfile()
        {
            CreateMap<BNPL_PlanSettlementSummary, BNPL_PlanSettlementSummaryResponseDto>()
                .ForMember(dest => dest.BNPL_PlanResponseDto, opt => opt.MapFrom(src => src.BNPL_PLAN));

            //No Request : because creation will be handled by backend
        }
    }
}