using AutoMapper;
using WebApplication1.DTOs.ResponseDto.BnplCal;

namespace WebApplication1.AutoMapperProfiles.BnplCal
{
    public class BNPLInstallmentCalculatorAutoMapperProfile : Profile
    {
        public BNPLInstallmentCalculatorAutoMapperProfile()
        {
            CreateMap<BNPLInstallmentCalculatorResultDto, BNPLInstallmentCalculatorResponseDto>()
                .ForMember(dest => dest.BNPL_PlanTypeResponseDto, opt => opt.MapFrom(src => src.BNPL_PlanType));
        }
    }
}