using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

using WebApplication1.Data;
using WebApplication1.AutoMapperProfiles;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Repositories.RepositoryImpl;
using WebApplication1.Services.IService;
using WebApplication1.Services.ServiceImpl;

var builder = WebApplication.CreateBuilder(args);

//--------------------[Swagger]--------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//--------------------[Controllers]----------------
builder.Services.AddControllers();

//--------------------[AutoMapper]-----------------
builder.Services.AddAutoMapper(
    typeof(PaginationMappingProfiles),

    typeof(BrandAutoMapperProfile),
    typeof(CategoryAutoMapperProfiles),
    typeof(CustomerAutoMapperProfiles),
    typeof(BNPL_InstallmentAutoMapperProfiles),
    typeof(BNPL_PlanAutoMapperProfiles),
    typeof(CustomerOrderAutoMapperProfiles),
    typeof(CustomerOrderElectronicItemAutoMapperProfiles),
    typeof(ElectronicItemAutoMapperProfiles),
    typeof(CashflowAutoMapperProfiles),
    typeof(BNPL_PlanTypeAutoMapperProfiles)
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
                .AddScoped<ICustomerRepository, CustomerRepositoryImpl>()
                .AddScoped<ICustomerOrderRepository, CustomerOrderRepositoryImpl>()
                .AddScoped<ICustomerOrderElectronicItemRepository, CustomerOrderElectronicItemRepositoryImpl>()
                .AddScoped<ICashflowRepository, CashflowRepositoryImpl>()
                .AddScoped<IBNPL_PlanTypeRepository, BNPL_PlanTypeRepositoryImpl>()
                .AddScoped<IBNPL_PlanRepository, BNPL_PlanRepositoryImpl>()
                .AddScoped<IBNPL_InstallmentRepository, BNPL_InstallmentRepositoryImpl>();

//--------------------[Services DI]-----------------
builder.Services.AddScoped<IBrandService, BrandServiceImpl>()
                .AddScoped<ICategoryService, CategoryServiceImpl>()
                .AddScoped<IFileService, FileServiceImpl>()
                .AddScoped<IElectronicItemService, ElectronicItemServiceImpl>()
                .AddScoped<ICustomerService, CustomerServiceImpl>()
                .AddScoped<ICustomerOrderService, CustomerOrderServiceImpl>()
                .AddScoped<ICashflowService, CashflowServiceImpl>()
                .AddScoped<IBNPL_PlanTypeService, BNPL_PlanTypeServiceImpl>()
                .AddScoped<IBNPL_PlanService, BNPL_PlanServiceImpl>()
                .AddScoped<IBNPL_InstallmentService, BNPL_InstallmentServiceImpl>();

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

app.MapControllers(); // Map controller routes

app.Run();