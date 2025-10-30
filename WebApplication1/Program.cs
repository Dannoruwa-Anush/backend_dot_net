using Microsoft.EntityFrameworkCore;
using AutoMapper;

using WebApplication1.Data;
using WebApplication1.AutoMapperProfiles;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Repositories.RepositoryImpl;
using WebApplication1.Services.IService;
using WebApplication1.Services.ServiceImpl;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer(); // For Swagger
builder.Services.AddSwaggerGen();           // For Swagger UI

// Add services to the container.
builder.Services.AddControllers();

// Add AutoMapper
builder.Services.AddAutoMapper(
    typeof(BrandAutoMapperProfile),
    typeof(CategoryAutoMapperProfiles),
    typeof(CustomerAutoMapperProfiles),
    typeof(BNPL_InstallmentAutoMapperProfiles),
    typeof(BNPL_PlanAutoMapperProfiles),
    typeof(CustomerOrderAutoMapperProfiles),
    typeof(CustomerOrderElectronicItemAutoMapperProfiles),
    typeof(ElectronicItemAutoMapperProfiles),
    typeof(PaymentAutoMapperProfiles),
    typeof(BNPL_PlanTypeAutoMapperProfiles)
);

// Configure EF Core (MySQL)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

//Register Repositories Layers (Dependency Injection)
builder.Services.AddScoped<IBrandRepository, BrandRepositoryImpl>()
                .AddScoped<ICategoryRepository, CategoryRepositoryImpl>()
                .AddScoped<IElectronicItemRepository, ElectronicItemRepositoryImpl>()
                .AddScoped<ICustomerRepository, CustomerRepositoryImpl>()
                .AddScoped<ICustomerOrderRepository, CustomerOrderRepositoryImpl>()
                .AddScoped<IBNPL_PlanTypeRepository, BNPL_PlanTypeRepositoryImpl>();

//Register Services Layers (Dependency Injection)
builder.Services.AddScoped<IBrandService, BrandServiceImpl>()
                .AddScoped<ICategoryService, CategoryServiceImpl>()
                .AddScoped<IElectronicItemService, ElectronicItemServiceImpl>()
                .AddScoped<ICustomerService, CustomerServiceImpl>()
                .AddScoped<IBNPL_PlanTypeService, BNPL_PlanTypeServiceImpl>();

var app = builder.Build();

//------------------[Start: Set default time zone globally]-----------------------
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
//------------------[End: Set default time zone globally]-------------------------

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers(); // Map controller routes

app.Run();