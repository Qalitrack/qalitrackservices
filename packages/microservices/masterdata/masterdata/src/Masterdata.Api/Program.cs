using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Security.Claims;
using FluentValidation;
using Serilog;
using Masterdata.Core.DTOs;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Mappings;
using Masterdata.Core.Services;
using Masterdata.Infrastructure.Data;
using Masterdata.Infrastructure.Repositories;
using Masterdata.Core.Entities;
using Masterdata.Core.Models;
using Masterdata.Core.Utils;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/masterdata-.txt", rollingInterval: RollingInterval.Day)
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
        Title = "Masterdata API", 
        Version = "v1",
        Description = @"QaliTrack Masterdata - MasterData API"
    });
    
    // Define the JWT Bearer scheme
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    
    // Apply the security requirement globally
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddDbContext<MasterdataDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
    "Host=localhost;Database=masterdatadb;Username=masterdata;Password=masterdata123"));

// Configure JWT Authentication
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? 
                builder.Configuration["JwtSettings:SecretKey"] ??
                throw new InvalidOperationException("JWT Secret Key is not configured.");

var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ??
             builder.Configuration["JwtSettings:Issuer"] ??
             "UserService";

var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ??
               builder.Configuration["JwtSettings:Audience"] ??
               "UserService";

// Validate secret key length
if (secretKey.Length < 32)
{
    throw new InvalidOperationException("JWT Secret Key must be at least 32 characters long for security.");
}

// Ensure proper encoding and key creation
var keyBytes = Encoding.UTF8.GetBytes(secretKey);
var key = new SymmetricSecurityKey(keyBytes);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; 
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = key,
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero, 
        NameClaimType = ClaimTypes.NameIdentifier,
        RoleClaimType = ClaimTypes.Role
    };

    // Add event handling for better debugging
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token validated successfully");
            return Task.CompletedTask;
        }
    };
});

// Configure Authorization
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();
});

// Register DbContext
builder.Services.AddDbContext<MasterdataDbContext>();

// Register HTTP context and memory cache first as they're used by other services
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

builder.Services.AddScoped<ITokenExtractionService, TokenExtractionService>();

// Register repositories with their dependencies
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IRepository<Driver>, Repository<Driver>>();
builder.Services.AddScoped<IRepository<Vehicle>, Repository<Vehicle>>();
builder.Services.AddScoped<IRepository<Supplier>, Repository<Supplier>>();

// Register specific repositories that have additional dependencies
builder.Services.AddScoped<IDriverVehicleRepository>(provider => 
    new DriverVehicleRepository(
        provider.GetRequiredService<MasterdataDbContext>(),
        provider.GetRequiredService<IHttpContextAccessor>(),
        provider.GetRequiredService<ITokenExtractionService>(),
        provider.GetRequiredService<IAuditLogRepository>()
    )
);

// Register services that depend on repositories
builder.Services.AddScoped<IDriverService, DriverService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
// Note: IDriverVehicleService is not implemented yet

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
// ALWAYS generate Swagger JSON (for both DEV + PROD)
app.UseSwagger();

// ALWAYS generate Swagger JSON
app.UseSwagger();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Masterdata API V1");
        c.SwaggerEndpoint("http://localhost:7000/swagger/v1/swagger.json", "API Gateway V1");
        c.RoutePrefix = string.Empty;
        c.DocumentTitle = "QaliTrack Services - Masterdata & Gateway Discovery";
    });
    
    // Debug middleware (KEEP)
    app.Use(async (context, next) =>
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader))
        {
            Console.WriteLine($"Auth Header: {authHeader}");
        }
        await next();
    });
}
else
{
    // ✅ PROD: Read-Only (blocks POST/PUT/DELETE via Traefik JWT)
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Masterdata API V1");
        c.RoutePrefix = string.Empty;
        c.DocumentTitle = "QaliTrack Masterdata API (Read-Only)";
    });
}
app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Use Serilog request logging
app.UseSerilogRequestLogging();

app.UseRouting();

// Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Initialize database
// Initialize database with proper error handling
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MasterdataDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Applying database migrations...");
        await context.Database.MigrateAsync(); // Use async version
        logger.LogInformation("Database migrations applied successfully.");
        
        logger.LogInformation("Seeding database...");
        await DatabaseSeeder.SeedAsync(context);
        logger.LogInformation("Database seeding completed successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        throw; // Re-throw to prevent app from starting with a broken database
    }
}

Log.Information("Masterdata starting up...");
app.Run();