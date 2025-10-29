using AutoMapper;
using WebApplication1.DTOs.RequestDto;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.Models;

namespace WebApplication1.AutoMapperProfiles
{
    public class BNPL_InstallmentAutoMapperProfiles : Profile
    {
        public BNPL_InstallmentAutoMapperProfiles()
        {
            CreateMap<BNPL_Installment, BNPL_InstallmentResponseDto>();
            CreateMap<BNPL_InstallmentRequestDto, BNPL_Installment>();
        }
    }
}