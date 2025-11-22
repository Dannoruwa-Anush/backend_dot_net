namespace WebApplication1.Utils.Project_Enums
{
    public enum BNPL_PlanSettlementSummary_StatusEnum
    {
        Active = 1,   // Latest and valid
        Obsolete = 2, // Not latest anymor
        Cancelled = 3 // Plan was cancelled
    }
}