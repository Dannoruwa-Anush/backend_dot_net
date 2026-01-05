using AutoMapper;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.Models;

namespace WebApplication1.AutoMapperProfiles
{
    public class InvoiceAutoMapperProfile : Profile
    {
        public InvoiceAutoMapperProfile()
        {
            CreateMap<Invoice, InvoiceResponseDto>()
                .ForMember(dest => dest.CustomerOrderResponseDto, opt => opt.MapFrom(src => src.CustomerOrder))
                .ForMember(dest => dest.CashflowResponseDto, opt => opt.MapFrom(src => src.Cashflow));
        }
    }
}