namespace WebApplication1.Utils.Project_Enums
{
    public enum BNPL_Installment_StatusEnum
    {
        Pending = 1,
        Paid_OnTime = 2,
        Paid_Late = 3,
        PartiallyPaid_OnTime = 4,
        PartiallyPaid_Late = 5,
        Overdue = 6,
        Cancelled = 7,
        Refunded = 8
    }
}