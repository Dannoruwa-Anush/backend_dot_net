using AutoMapper;
using WebApplication1.DTOs.RequestDto;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.Models;

namespace WebApplication1.AutoMapperProfiles
{
    public class PaymentAutoMapperProfiles : Profile
    {
        public PaymentAutoMapperProfiles()
        {
            CreateMap<Payment, PaymentResponseDto>();
            CreateMap<PaymentRequestDto, Payment>();
        }   
    }
}