using WebApplication1.Models;

namespace WebApplication1.DTOs.ResponseDto.BnplCal{
    public class BNPLInstallmentCalculatorResultDto
    {
        // Include simplified info about Bnpl_PlanType
        public required BNPL_PlanType BNPL_PlanType { get; set; }

        // Calculated Values
        public decimal AmountPerInstallment { get; set; }
        public decimal TotalPayable { get; set; }
        public decimal TotalInterestAmount { get; set; }
    }
}