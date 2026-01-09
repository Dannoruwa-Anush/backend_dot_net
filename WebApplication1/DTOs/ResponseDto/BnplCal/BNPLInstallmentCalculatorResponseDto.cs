namespace WebApplication1.DTOs.ResponseDto.BnplCal{
    public class BNPLInstallmentCalculatorResponseDto
    {
        // Include simplified info about Bnpl_PlanType
        public required BNPL_PlanTypeResponseDto BnplPlanTypeResponseDto { get; set; }

        // Calculated Values
        public decimal AmountPerInstallment { get; set; }
        public decimal TotalPayable { get; set; }
        public decimal TotalInterestAmount { get; set; }
    }
}