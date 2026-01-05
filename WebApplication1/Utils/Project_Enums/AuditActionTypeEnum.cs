namespace WebApplication1.Utils.Project_Enums
{
    public enum AuditActionTypeEnum
    {
        Create = 1,
        Update = 2,
        Delete = 3,

        // Auth-specific
        Register = 4,
        LoginSuccess = 5,
        LoginFailure = 6,
        Logout = 7
    }
}