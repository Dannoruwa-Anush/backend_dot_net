using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using WebApplication1.Services.IService.Auth;
using WebApplication1.Utils.Records;

namespace WebApplication1.Data
{
    //Note: Design-time DbContext Factory

    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseMySql(
                config.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(config.GetConnectionString("DefaultConnection"))
            );

            // Provide a dummy ICurrentUserService for design-time
            return new AppDbContext(optionsBuilder.Options, new NullCurrentUserService());
        }

        private class NullCurrentUserService : ICurrentUserService
        {
            public int? UserID => null;
            public string? Email => null;
            public string? Role => null;
            public string? EmployeePosition => null;
            public ClaimsPrincipal? User => null;
            public CurrentUserProfile UserProfile => new CurrentUserProfile(null, null, null, null); // empty profile
        }
    }
}