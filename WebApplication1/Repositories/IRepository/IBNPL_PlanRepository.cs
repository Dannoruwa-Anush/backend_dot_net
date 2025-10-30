namespace WebApplication1.Repositories.IRepository
{
    public interface IBNPL_PlanRepository
    {
        //CRUD operations

        //Custom Query Operations
        Task<bool> ExistsByBnplPlanTypeAsync(int bnplPlanTypeId);
    }
}