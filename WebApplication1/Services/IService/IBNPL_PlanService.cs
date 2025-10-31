using WebApplication1.DTOs.RequestDto.BnplCal;
using WebApplication1.DTOs.ResponseDto.BnplCal;

namespace WebApplication1.Services.IService
{
    public interface IBNPL_PlanService
    {
        //CRUD operations

        //Custom Query Operations
        Task<BNPLInstallmentCalculatorResponseDto> CalculateAmountPerInstallmentAsync(BNPLInstallmentCalculatorRequestDto request);
    }
}