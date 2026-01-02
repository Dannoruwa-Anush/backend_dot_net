using AutoMapper;
using WebApplication1.DTOs.RequestDto;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.Models;

namespace WebApplication1.AutoMapperProfiles
{
    public class CashflowAutoMapperProfiles : Profile
    {
        public CashflowAutoMapperProfiles()
        {
            CreateMap<Cashflow, CashflowResponseDto>()
              .ForMember(dest => dest.CustomerOrderResponseDto, opt => opt.MapFrom(src => src.CustomerOrder));
        }   
    }
}