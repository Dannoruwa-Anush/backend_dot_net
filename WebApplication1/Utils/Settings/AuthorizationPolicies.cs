namespace WebApplication1.Utils.Settings
{
    public static class AuthorizationPolicies
    {
        public const string AdminOnly = "AdminOnly";
        public const string CustomerOnly = "CustomerOnly";
        public const string AllEmployeesOnly = "AllEmployeesOnly";
        public const string ManagerOnly = "ManagerOnly";
        public const string CashierOnly = "CashierOnly";
        public const string CashierOrCustomer = "CashierOrCustomer";
    }
}