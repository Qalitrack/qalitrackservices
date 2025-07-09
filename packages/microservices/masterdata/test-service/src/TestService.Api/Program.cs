using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FluentValidation;
using Serilog;
using TestService.Core.Interfaces;
using TestService.Core.Services;
using TestService.Core.DTOs;
// using TestService.Core.Validators;
using TestService.Core.Mappings;
using TestService.Infrastructure.Data;
using TestService.Infrastructure.Repositories;
// using TestService.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/test-service-.txt", rollingInterval: RollingInterval.Day)
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
        Title = "TestService API", 
        Version = "v1",
        Description = @"QaliTrack TestService - TestService Management API

🌐 **Gateway Information:**
- **Gateway URL**: http://localhost:7000
- **Gateway Health**: http://localhost:7000/health  
- **Gateway Service Discovery**: http://localhost:7000/api/gateway/services
- **Gateway Info**: http://localhost:7000/api/gateway/info

📋 **Available Routes via Gateway:**
- All TestService endpoints are also available via Gateway at http://localhost:7000/api/items/*
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
builder.Services.AddAutoMapper(typeof(ItemProfile));

// Add FluentValidation
// builder.Services.AddValidatorsFromAssemblyContaining<ItemRequestValidator>();

// Add Entity Framework
builder.Services.AddDbContext<TestServiceDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
    "Data Source=test-service.db"));

// TODO: Add authentication if needed for this service
// For authentication services, uncomment and configure JWT
/*
// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
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
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();
*/

// Add repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IItemRepository, ItemRepository>();
// TODO: Add additional repositories as needed
// builder.Services.AddScoped<IAnotherRepository, AnotherRepository>();

// Add services
builder.Services.AddScoped<IItemService, ItemService>();
// TODO: Add additional services as needed
// builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
// builder.Services.AddScoped<IJwtService, JwtService>();
// builder.Services.AddScoped<IPasswordService, PasswordService>();
// builder.Services.AddScoped<IEmailService, EmailService>();

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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TestService API V1");
        c.SwaggerEndpoint("http://localhost:7000/swagger/v1/swagger.json", "API Gateway V1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
        c.DocumentTitle = "QaliTrack Services - TestService & Gateway Discovery";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Use Serilog request logging
app.UseSerilogRequestLogging();

// TODO: Uncomment if authentication is needed
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TestServiceDbContext>();
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

Log.Information("TestService starting up...");
app.Run();

// Make Program class accessible for testing
public partial class Program { }
