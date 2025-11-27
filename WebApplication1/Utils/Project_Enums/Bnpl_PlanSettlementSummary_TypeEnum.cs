namespace WebApplication1.Utils.Project_Enums
{
    public enum Bnpl_PlanSettlementSummary_TypeEnum
    {
        Initial = 1,          // Indicates: Snapshot created during new BNPL plan + installment creation
        AfterPayment = 2,     // Indicates: Snapshot created after recalculating accumulated settlement due to a payment
        AfterLateInterest = 3 // Indicates: Snapshot created after applying late interest (handling overdue installments)
    }
}