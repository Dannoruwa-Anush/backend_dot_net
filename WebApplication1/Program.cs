using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

using WebApplication1.Data;
using WebApplication1.AutoMapperProfiles;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Repositories.RepositoryImpl;
using WebApplication1.Services.IService;
using WebApplication1.Services.ServiceImpl;
using WebApplication1.Utils.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApplication1.Services.IService.Auth;
using WebApplication1.AutoMapperProfiles.Auth;
using WebApplication1.Services.ServiceImpl.Auth;
using System.Security.Claims;
using WebApplication1.UOW.IUOW;
using WebApplication1.UOW.UOWImpl;
using WebApplication1.Services.IService.Helper;
using WebApplication1.Services.ServiceImpl.Helper;
using WebApplication1.AutoMapperProfiles.BnplCal;

var builder = WebApplication.CreateBuilder(args);

//--------------------[Swagger]--------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//--------------------[Controllers]----------------
builder.Services.AddControllers();

//--------------------[AutoMapper]-----------------
builder.Services.AddAutoMapper(
    typeof(PaginationAutoMappingProfiles), //This is a generic AutoMapper

    typeof(AuthAutoMapperProfile),
    typeof(UserAutoMappingProfile),
    typeof(EmployeeAutoMappingProfile),
    typeof(CustomerAutoMapperProfiles),

    typeof(BrandAutoMapperProfile),
    typeof(CategoryAutoMapperProfiles),
    typeof(ElectronicItemAutoMapperProfiles),
    typeof(CustomerOrderAutoMapperProfiles),
    typeof(CustomerOrderElectronicItemAutoMapperProfiles),
    
    typeof(CashflowAutoMapperProfiles),
    typeof(BNPL_PlanTypeAutoMapperProfiles),
    typeof(BNPL_PlanAutoMapperProfiles),
    typeof(BNPL_InstallmentAutoMapperProfiles),
    typeof(BNPL_PlanSettlementSummaryAutoMapperProfile),
    typeof(PhysicalShopSessionAutoMapperProfile),

    //Helpers
    typeof(BNPLInstallmentCalculatorAutoMapperProfile)
);

//--------------------[EF Core - MySQL]-------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

//--------------------[Repositories DI]-------------
builder.Services.AddScoped<IBrandRepository, BrandRepositoryImpl>()
                .AddScoped<ICategoryRepository, CategoryRepositoryImpl>()
                .AddScoped<IElectronicItemRepository, ElectronicItemRepositoryImpl>()
                .AddScoped<IUserRepository, UserRepositoryImpl>()
                .AddScoped<IEmployeeRepository, EmployeeRepositoryImpl>()
                .AddScoped<ICustomerRepository, CustomerRepositoryImpl>()
                .AddScoped<ICustomerOrderRepository, CustomerOrderRepositoryImpl>()
                .AddScoped<ICashflowRepository, CashflowRepositoryImpl>()
                .AddScoped<IBNPL_PlanTypeRepository, BNPL_PlanTypeRepositoryImpl>()
                .AddScoped<IBNPL_PlanRepository, BNPL_PlanRepositoryImpl>()
                .AddScoped<IBNPL_InstallmentRepository, BNPL_InstallmentRepositoryImpl>()
                .AddScoped<IBNPL_PlanSettlementSummaryRepository, BNPL_PlanSettlementSummaryRepositoryImpl>()
                .AddScoped<IPhysicalShopSessionRepository, PhysicalShopSessionRepositoryImpl>();

//--------------------[Unit of work DI]------------
builder.Services.AddScoped<IAppUnitOfWork, AppUnitOfWorkImpl>();

//--------------------[Http Context Accessor DI]------------
builder.Services.AddHttpContextAccessor(); // Needed for CurrentUserService

//--------------------[Services DI]-----------------
builder.Services.AddScoped<IAuthService, AuthServiceImpl>()
                .AddScoped<ICurrentUserService, CurrentUserServiceImpl>()

                .AddScoped<IFileService, FileServiceImpl>()
                .AddScoped<IDocumentGenerationService, DocumentGenerationServiceImpl>()

                .AddScoped<IBrandService, BrandServiceImpl>()
                .AddScoped<ICategoryService, CategoryServiceImpl>()
                .AddScoped<IElectronicItemService, ElectronicItemServiceImpl>()
                .AddScoped<IEmployeeService, EmployeeServiceImpl>()
                .AddScoped<ICustomerService, CustomerServiceImpl>()
                .AddScoped<ICustomerOrderService, CustomerOrderServiceImpl>()
                .AddScoped<ICashflowService, CashflowServiceImpl>()
                .AddScoped<IBNPL_PlanTypeService, BNPL_PlanTypeServiceImpl>()
                .AddScoped<IBNPL_PlanService, BNPL_PlanServiceImpl>()
                .AddScoped<IBNPL_InstallmentService, BNPL_InstallmentServiceImpl>()
                .AddScoped<IBNPL_PlanSettlementSummaryService, BNPL_PlanSettlementSummaryServiceImpl>()
                .AddScoped<IPaymentService, PaymentServiceImpl>()
                .AddScoped<IDueDateAdjustmentService, DueDateAdjustmentServiceImpl>()
                .AddScoped<IPhysicalShopSessionService, PhysicalShopSessionServiceImpl>();

//--------------------[Configure JWT authentication]-----------------------
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT configuration section 'Jwt' is missing or invalid.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            RoleClaimType = ClaimTypes.Role
        };

        // Add custom behavior when token is missing or invalid
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                // Suppress the default 401 response
                context.HandleResponse();

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                // Redirect browser users (Redirect to : frontend /login)
                context.Response.Headers["Location"] = "/login";

                // Send JSON message as well
                await context.Response.WriteAsync(
                    System.Text.Json.JsonSerializer.Serialize(new
                    {
                        message = "Unauthorized: please log in to continue."
                    })
                );
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    status = 403,
                    message = "You don't have access to this resource."
                });
            }
        };
    }
);

builder.Services
    .AddAuthorizationBuilder()
    .AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
        policy.RequireRole("Admin"))
    .AddPolicy(AuthorizationPolicies.CustomerOnly, policy =>
        policy.RequireRole("Customer"))
    .AddPolicy(AuthorizationPolicies.AllEmployeesOnly, policy =>
        policy.RequireRole("Employee"))
    .AddPolicy(AuthorizationPolicies.ManagerOnly, policy =>
        policy.RequireRole("Employee")
              .RequireClaim("EmployeePosition", "Manager"))
    .AddPolicy(AuthorizationPolicies.CashierOnly, policy =>
        policy.RequireRole("Employee")
              .RequireClaim("EmployeePosition", "Cashier"))
    .AddPolicy(AuthorizationPolicies.CashierOrCustomer, policy =>
        policy.RequireAssertion(context =>
            (context.User.IsInRole("Employee") &&
             context.User.HasClaim("EmployeePosition", "Cashier")) ||
            context.User.IsInRole("Customer")
        ));

// -------------[CORS for Angular]--------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") // Angular
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});

//----------------------------------------------------

var app = builder.Build();

//------------------[Set default time zone globally]-----------------------
TimeZoneInfo sriLankaZone;
try
{
    // Works on Windows
    sriLankaZone = TimeZoneInfo.FindSystemTimeZoneById("Sri Lanka Standard Time");
}
catch
{
    // Works on Linux/macOS
    sriLankaZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Colombo");
}
TimeZoneInfo.ClearCachedData();
TimeZoneInfo.Local.Equals(sriLankaZone);

// -------------------- Ensure wwwroot & uploads/images exist --------------------
var webRootPath = builder.Environment.WebRootPath;
if (string.IsNullOrEmpty(webRootPath))
{
    webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    Directory.CreateDirectory(webRootPath);
}

// Create uploads/images folder
var uploadsFolder = Path.Combine(webRootPath, "uploads/images");
Directory.CreateDirectory(uploadsFolder);

// -------------------- Middleware --------------------------------
//------------------[HTTP request pipeline]------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngularClient"); // Enable CORS BEFORE auth

app.UseAuthentication(); // Required before authorization
app.UseAuthorization();

//------------------[Static file serving]------------------------
// Serve wwwroot
app.UseStaticFiles();

// Serve uploads/images folder explicitly
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.WebRootPath, "uploads/images")
    ),
    RequestPath = "/uploads/images"
});

// Make everything require auth by default
app.MapControllers().RequireAuthorization();

app.Run();