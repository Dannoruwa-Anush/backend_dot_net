namespace WebApplication1.Utils.Project_Enums
{
    public enum BnplStatusEnum
    {
        Requested = 1,  // BNPL order created, awaiting initial payment
        Active = 2,     // Initial payment completed, BNPL plan is in active
        Completed = 3,  // BNPL plan fully paid and completed
        Cancelled = 4,  // BNPL plan cancelled due to order cancellation
        Defaulted = 5,  // Payment not completed on time, plan defaulted
    }
}