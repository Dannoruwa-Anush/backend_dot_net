namespace WebApplication1.Utils.Records
{
    // Note: A record is immutable and perfect for data containers.
    public record CurrentUserProfile(
        int? UserID,
        string? Email,
        string? Role,
        string? EmployeePosition
    );
}