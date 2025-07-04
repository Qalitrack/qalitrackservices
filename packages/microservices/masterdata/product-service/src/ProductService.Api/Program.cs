using Microsoft.EntityFrameworkCore;
using ProductService.Core.Interfaces;
using ProductService.Core.Services;
using ProductService.Infrastructure.Data;
using ProductService.Infrastructure.Repositories;
using ProductService.Core.Entities;
using FluentValidation.AspNetCore;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ProductService.Core.Validators.CreateProductCategoryValidator>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(ProductService.Core.Mappings.ProductProfile));

// Add Entity Framework
if (builder.Environment.EnvironmentName != "Testing")
{
    builder.Services.AddDbContext<ProductDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
        "Data Source=productservice.db"));
}

// Add repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
builder.Services.AddScoped<IProductComplianceRepository, ProductComplianceRepository>();

// Add services
builder.Services.AddScoped<IProductService, ProductService.Core.Services.ProductService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();

// Make Program class accessible to test projects
public partial class Program { }