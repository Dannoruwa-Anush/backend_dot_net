using AutoMapper;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.Models;

namespace WebApplication1.AutoMapperProfiles
{
    public class InvoiceAutoMapperProfile : Profile
    {
        public InvoiceAutoMapperProfile()
        {
            CreateMap<Invoice, InvoiceResponseDto>();
        }
    }
}