namespace WebApplication1.DTOs.RequestDto.BnplCal
{
    public class BNPLInstallmentCalculatorRequestDto
    {
        public int OrderID { get; set; }              
        public decimal InitialPayment { get; set; }             
        public int Bnpl_PlanTypeID { get; set; }                   
        public int InstallmentCount { get; set; }               
    }
}