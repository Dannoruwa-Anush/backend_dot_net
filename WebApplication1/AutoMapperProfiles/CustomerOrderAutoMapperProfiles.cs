using AutoMapper;
using WebApplication1.DTOs.RequestDto;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.Models;
using WebApplication1.Utils.Project_Enums;
using WebApplication1.Utils.SystemConstants;

namespace WebApplication1.AutoMapperProfiles
{
    public class CustomerOrderAutoMapperProfiles : Profile
    {
        public CustomerOrderAutoMapperProfiles()
        {
            CreateMap<CustomerOrder, CustomerOrderResponseDto>()
                .ForMember(dest => dest.CustomerResponseDto, opt => opt.MapFrom(src => src.Customer))
                .ForMember(dest => dest.CustomerOrderElectronicItemResponseDto, opt => opt.MapFrom(src => src.CustomerOrderElectronicItems))
                //Include latestUnpaid invoice for the order
                .ForMember(dest => dest.LatestUnpaidInvoice, opt => opt.MapFrom(src => src.Invoices.Where(i => i.InvoiceStatus == InvoiceStatusEnum.Unpaid).OrderByDescending(i => i.InvoiceID).FirstOrDefault()))
                // Calculate if free trial is over
                .ForMember(dest => dest.IsFreeTrialOver, opt => opt.MapFrom(src => (DateTime.UtcNow - src.OrderDate).TotalDays > BnplSystemConstants.FreeTrialPeriodDays));

            CreateMap<CustomerOrderRequestDto, CustomerOrder>();
        }
    }
}