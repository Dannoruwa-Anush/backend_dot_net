namespace WebApplication1.DTOs.ResponseDto.BnplCal{
    public class BNPLInstallmentCalculatorResponseDto
    {
        public decimal InterestRate { get; set; }
        public decimal LatePayInterestRate { get; set; }
        public string PlanTypeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal AmountPerInstallment { get; set; }
        public decimal TotalPayable { get; set; }
        public decimal TotalInterestAmount { get; set; }
    }
}