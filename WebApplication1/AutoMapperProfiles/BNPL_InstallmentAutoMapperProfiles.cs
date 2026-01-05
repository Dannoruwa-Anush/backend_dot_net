using AutoMapper;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.Models;

namespace WebApplication1.AutoMapperProfiles
{
    public class BNPL_InstallmentAutoMapperProfiles : Profile
    {
        public BNPL_InstallmentAutoMapperProfiles()
        {
            CreateMap<BNPL_Installment, BNPL_InstallmentResponseDto>()
                .ForMember(dest => dest.BNPL_PlanResponseDto, opt => opt.MapFrom(src => src.BNPL_PLAN));
        }
    }
}