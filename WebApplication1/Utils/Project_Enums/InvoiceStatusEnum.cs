namespace WebApplication1.Utils.Project_Enums
{
    public enum InvoiceStatusEnum
    {
        Draft = 1,      // Created, no payment yet
        Paid = 2,       // Payment recieved
        Refunded = 3,   // Paid, then refunded (credit issued)
        Cancelled = 4   // Draft invoice voided (no payment)
    }
}