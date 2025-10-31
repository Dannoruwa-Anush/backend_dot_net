namespace WebApplication1.DTOs.ResponseDto
{
    public class BNPL_PlanTypeResponseDto
    {
        public int Bnpl_PlanTypeID { get; set; }
        public string Bnpl_PlanTypeName { get; set; } = string.Empty;
        public int Bnpl_DurationDays { get; set; }
        public decimal InterestRate { get; set; }
        public decimal LatePayInterestRate { get; set; }
        public string Bnpl_Description { get; set; } = string.Empty;
    }
}