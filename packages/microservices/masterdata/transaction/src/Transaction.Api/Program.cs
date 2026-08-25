using Microsoft.AspNetCore.Authentication.JwtBearer;
using Prometheus;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Transaction.Core.Interfaces;
using Transaction.Core.Services;
using Transaction.Core.DTOs;
using Transaction.Infrastructure.Services;
// using Transaction.Core.Validators;
using Transaction.Core.Mappings;
using Transaction.Infrastructure.Data;
using Transaction.Infrastructure.Repositories;
using Transaction.Api.Middleware;
// using Transaction.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/transaction-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Kestrel for larger file uploads
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 10485760; // 10MB
});

// Configure FormOptions for multipart/form-data
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10485760; // 10MB
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

// Configure Swagger with JWT authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Transaction API", 
        Version = "v1",
        Description = @"QaliTrack Transaction - Transaction API

🌐 **Gateway Information:**
- **Gateway URL**: http://localhost:7000
- **Gateway Health**: http://localhost:7000/health  
- **Gateway Service Discovery**: http://localhost:7000/api/gateway/services
- **Gateway Info**: http://localhost:7000/api/gateway/info

📋 **Available Routes via Gateway:**
- All Transaction endpoints are also available via Gateway at http://localhost:7000/api/transactions/*
- Gateway provides centralized routing to 18+ microservices including analytics, compliance, transactions, and more"
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
builder.Services.AddAutoMapper(typeof(TransactionProfile));

// Add FluentValidation
// builder.Services.AddValidatorsFromAssemblyContaining<TransactionRequestValidator>();

// Add Entity Framework with PostgreSQL
builder.Services.AddDbContext<TransactionDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                           "Host=localhost;Database=qalitrackdb;Username=postgres;Password=postgres";

    options.UseNpgsql(connectionString, npgsqlOptions =>
        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "transactions"));

    // Disable lazy loading to prevent AutoMapper issues with proxies
    options.UseLazyLoadingProxies(false);
});

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
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionSettingsRepository, TransactionSettingsRepository>();
// TODO: Add additional repositories as needed
// builder.Services.AddScoped<IAnotherRepository, AnotherRepository>();

// Add TimeService for East African Time (Nairobi, UTC+3)
builder.Services.AddSingleton<ITimeService, TimeService>();

// Add services
builder.Services.AddScoped<ITransactionService, Transaction.Core.Services.TransactionService>();
builder.Services.AddScoped<IReceiptNumberService, ReceiptNumberService>();
builder.Services.AddScoped<ITransactionSettingsService, TransactionSettingsService>();
builder.Services.AddScoped<IFileStorageService, FileSystemStorageService>();
// TODO: Add additional services as needed
// builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
// builder.Services.AddScoped<IJwtService, JwtService>();
// builder.Services.AddScoped<IPasswordService, PasswordService>();
// builder.Services.AddScoped<IEmailService, EmailService>();

// Add CORS - Allow everything
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Content-Disposition"); // For file downloads
    });
});

// Add Health Checks
builder.Services.AddHealthChecks();

// Ensure uploads directory exists
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
    Directory.CreateDirectory(Path.Combine(uploadsPath, "transactions"));
}

var app = builder.Build();

// Serve uploaded files
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads",
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
});

// Enable directory browsing for uploads (only in development)
if (app.Environment.IsDevelopment())
{
    app.UseDirectoryBrowser(new DirectoryBrowserOptions
    {
        FileProvider = new PhysicalFileProvider(uploadsPath),
        RequestPath = "/uploads"
    });
}

// Configure the HTTP request pipeline
// ALWAYS generate Swagger JSON (for both DEV + PROD)
app.UseSwagger();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Transaction API V1");
        c.SwaggerEndpoint("http://localhost:7000/swagger/v1/swagger.json", "API Gateway V1");
        c.RoutePrefix = string.Empty;
        c.DocumentTitle = "QaliTrack Services - Transaction & Gateway Discovery";
    });
}
else
{
    // PROD: Serve UI at /swagger
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("v1/swagger.json", "Transaction API V1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "QaliTrack Transaction API";
    });
}

// ⚠️ CRITICAL: Enable CORS middleware - This line was MISSING!
app.UseCors("AllowAll");

// Use Data Leak Prevention Middleware (sanitizes errors and sensitive data)
if (!app.Environment.IsEnvironment("Test"))
{
    app.UseDataLeakPrevention();
}

// Use Serilog request logging
if (!app.Environment.IsEnvironment("Test"))
{
    app.UseSerilogRequestLogging();
}

app.UseHttpMetrics();

// Use authentication and authorization if needed
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapMetrics();

// Run database migrations automatically on startup in non-Development environments
// Run database migrations automatically on startup
if (!app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
        try
        {
            Log.Information("Applying database migrations...");
            
            // Only migrate if using a real database (not InMemory)
            if (dbContext.Database.IsRelational())
            {
                dbContext.Database.Migrate();
                Log.Information("Database migrations applied successfully");
            }
            else
            {
                Log.Information("Using non-relational database, skipping migrations");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error applying database migrations");
            throw;
        }
    }
}
else
{
    // In Development, just ensure the database is created
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
        
        // Only for relational databases
        if (dbContext.Database.IsRelational())
        {
            dbContext.Database.EnsureCreated();
        }
    }
}

Log.Information("Transaction starting up...");
app.Run();

// Make Program class accessible for testing
public partial class Program { }