using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FluentValidation;
using Serilog;
using TestServiceV4.Core.Interfaces;
using TestServiceV4.Core.Services;
using TestServiceV4.Core.DTOs;
using TestServiceV4.Core.Validators;
using TestServiceV4.Core.Mappings;
using TestServiceV4.Infrastructure.Data;
using TestServiceV4.Infrastructure.Repositories;
using TestServiceV4.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/test-service-v4-.txt", rollingInterval: RollingInterval.Day)
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
        Title = "TestServiceV4 API", 
        Version = "v1",
        Description = @"QaliTrack TestServiceV4 - TestServiceV4 Management API

🌐 **Gateway Information:**
- **Gateway URL**: http://localhost:7000
- **Gateway Health**: http://localhost:7000/health  
- **Gateway Service Discovery**: http://localhost:7000/api/gateway/services
- **Gateway Info**: http://localhost:7000/api/gateway/info

📋 **Available Routes via Gateway:**
- All TestServiceV4 endpoints are also available via Gateway at http://localhost:7000/api/testentitys/*
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
builder.Services.AddAutoMapper(typeof(TestentityProfile));

// Add FluentValidation
// builder.Services.AddValidatorsFromAssemblyContaining<TestentityRequestValidator>();

// Add Entity Framework
builder.Services.AddDbContext<TestServiceV4DbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
    "Data Source=test-service-v4.db"));

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
builder.Services.AddScoped<ITestentityRepository, TestentityRepository>();
// TODO: Add additional repositories as needed
// builder.Services.AddScoped<IAnotherRepository, AnotherRepository>();

// Add services
builder.Services.AddScoped<ITestentityService, TestentityService>();
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TestServiceV4 API V1");
        c.SwaggerEndpoint("http://localhost:7000/swagger/v1/swagger.json", "API Gateway V1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
        c.DocumentTitle = "QaliTrack Services - TestServiceV4 & Gateway Discovery";
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
    var dbContext = scope.ServiceProvider.GetRequiredService<TestServiceV4DbContext>();
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

Log.Information("TestServiceV4 starting up...");
app.Run();

// Make Program class accessible for testing
public partial class Program { }
