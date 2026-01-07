namespace WebApplication1.Utils.Project_Enums
{
    public enum InvoiceStatusEnum
    {
        Opened = 1,        // Invoice created, stock allocated, BNPL created, awaiting payment
        Voided = 2,      // Invoice created but cancelled before payment (stock released)
        Paid = 3,        // Payment received
        Refunded = 4     // Paid, then refunded
    }
}