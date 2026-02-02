using AutoMapper;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.Models;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.AutoMapperProfiles
{
    public class InvoiceAutoMapperProfile : Profile
    {
        public InvoiceAutoMapperProfile()
        {
            CreateMap<Invoice, InvoiceResponseDto>()
                .ForMember(
                    dest => dest.CashflowResponseDtos,
                    opt => opt.MapFrom(src =>
                        src.Cashflows.Where(c =>
                            c.CashflowPaymentNature == CashflowPaymentNatureEnum.Payment ||
                            c.CashflowPaymentNature == CashflowPaymentNatureEnum.Refund)
                    )
                );
        }
    }
}