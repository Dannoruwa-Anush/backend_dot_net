namespace WebApplication1.DTOs.ResponseDto.LateInterest
{
    public class LateInterestCalculationResultDto
    {
        public int OverdueDays { get; set; }
        public decimal UnpaidBase { get; set; }
        public decimal InterestToAdd { get; set; }
        public decimal NewLateInterestTotal { get; set; }
        public decimal NewTotalDueAmount { get; set; }
    }
}