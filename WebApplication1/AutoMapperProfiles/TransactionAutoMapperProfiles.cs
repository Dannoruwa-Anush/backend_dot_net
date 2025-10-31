using AutoMapper;
using WebApplication1.DTOs.RequestDto;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.Models;

namespace WebApplication1.AutoMapperProfiles
{
    public class TransactionAutoMapperProfiles : Profile
    {
        public TransactionAutoMapperProfiles()
        {
            CreateMap<Transaction, TransactionResponseDto>();
            CreateMap<TransactionRequestDto, Transaction>();
        }   
    }
}