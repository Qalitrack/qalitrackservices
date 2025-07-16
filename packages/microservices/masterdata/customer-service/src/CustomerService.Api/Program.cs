using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FluentValidation;
using Serilog;
using CustomerService.Core.Interfaces;
using CustomerService.Core.Services;
using CustomerService.Core.DTOs;
// using CustomerService.Core.Validators;
using CustomerService.Core.Mappings;
using CustomerService.Infrastructure.Data;
using CustomerService.Infrastructure.Repositories;
// using CustomerService.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/customer-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "CustomerService API", 
        Version = "v1",
        Description = @"QaliTrack CustomerService - Customer Relationship Management API

🌐 **Gateway Information:**
- **Gateway URL**: http://localhost:7000
- **Gateway Health**: http://localhost:7000/health  
- **Gateway Service Discovery**: http://localhost:7000/api/gateway/services
- **Gateway Info**: http://localhost:7000/api/gateway/info

📋 **Available Routes via Gateway:**
- All CustomerService endpoints are also available via Gateway at http://localhost:7000/api/customers/*
- Gateway provides centralized routing to 18+ microservices including analytics, compliance, transactions, and more"
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(CustomerProfile));

// Add FluentValidation
// builder.Services.AddValidatorsFromAssemblyContaining<CustomerRequestValidator>();

// Add Entity Framework
builder.Services.AddDbContext<CustomerServiceDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
    "Data Source=customer-service.db"));

// Add JWT Authentication for User Service integration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "QaliTrack",
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"] ?? "QaliTrack-Services",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Add repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Add services
builder.Services.AddScoped<ICustomerService, CustomerService.Core.Services.CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IContractService, ContractService>();

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

// Add Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CustomerService API V1");
        c.SwaggerEndpoint("http://localhost:7000/swagger/v1/swagger.json", "API Gateway V1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
        c.DocumentTitle = "QaliTrack Services - CustomerService & Gateway Discovery";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Use Serilog request logging
app.UseSerilogRequestLogging();

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CustomerServiceDbContext>();
    try
    {
        dbContext.Database.EnsureCreated();
        Log.Information("Database ensured created successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error creating database");
        throw;
    }
}

Log.Information("CustomerService starting up...");
app.Run();

// Make Program class accessible for testing
public partial class Program { }
