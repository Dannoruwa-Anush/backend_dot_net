namespace WebApplication1.Utils.Project_Enums
{
    public enum OrderStatusEnum
    {
        Pending = 1,    // Stock allocated, Invoice Opened, BNPL created, awaiting payment
        Processing = 2, // After the payment recieved, order start to process
        Shipped = 3,
        Delivered = 4,
        Cancel_Pending = 5, // requested by customer/cashier
        Cancelled = 6,      // by manager (stock released)
        DeliveredAfterCancellationRejected = 7, // by manager
    }
}