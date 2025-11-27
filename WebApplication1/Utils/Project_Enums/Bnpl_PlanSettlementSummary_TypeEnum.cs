namespace WebApplication1.Utils.Project_Enums
{
    public enum Bnpl_PlanSettlementSummary_TypeEnum
    {
        Initial = 1,          // Indicates: Snapshot created during new BNPL plan + installment creation
        AfterLateInterest = 2 // Indicates: Snapshot created after applying late interest (handling overdue installments)
    }
}