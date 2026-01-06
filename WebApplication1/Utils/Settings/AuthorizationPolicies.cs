namespace WebApplication1.Utils.Settings
{
    public static class AuthorizationPolicies
    {
        // user level
        public const string AdminOnly = "AdminOnly";
        public const string CustomerOnly = "CustomerOnly";
        public const string AllEmployeesOnly = "AllEmployeesOnly";

        // Employee position level
        public const string ManagerOnly = "ManagerOnly";
        public const string CashierOnly = "CashierOnly";

        // User or Employee
        public const string AdminOrManager = "AdminOrManager";
        public const string CustomerOrCashier = "CustomerOrCashier";
    }
}