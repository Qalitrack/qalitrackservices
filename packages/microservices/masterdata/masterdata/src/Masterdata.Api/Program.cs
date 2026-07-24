using Microsoft.AspNetCore.Authentication.JwtBearer;
using Npgsql;
using Prometheus;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Security.Claims;
using FluentValidation;
using Masterdata.Api.Extensions;
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

// Configure PostgreSQL connection
var dbHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? 
             builder.Configuration.GetValue<string>("ConnectionStrings:DefaultConnection:Host") ?? 
             "postgres-masterdata-prod";

var dbPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? 
             builder.Configuration.GetValue<string>("ConnectionStrings:DefaultConnection:Port") ?? 
             "5432";

var dbName = Environment.GetEnvironmentVariable("POSTGRES_DATABASE") ?? 
             builder.Configuration.GetValue<string>("ConnectionStrings:DefaultConnection:Database") ?? 
             "qalitrack_masterdata";

var dbUser = Environment.GetEnvironmentVariable("POSTGRES_USERNAME") ?? 
             builder.Configuration.GetValue<string>("ConnectionStrings:DefaultConnection:Username") ?? 
             "masterdata";

var dbPass = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? 
             builder.Configuration.GetValue<string>("ConnectionStrings:DefaultConnection:Password") ?? 
             "masterdata123";

// Validate required environment variables
if (string.IsNullOrWhiteSpace(dbHost)) throw new Exception("Database host is not configured");
if (string.IsNullOrWhiteSpace(dbName)) throw new Exception("Database name is not configured");
if (string.IsNullOrWhiteSpace(dbUser)) throw new Exception("Database user is not configured");
if (string.IsNullOrWhiteSpace(dbPass)) throw new Exception("Database password is not configured");

// Build connection string
var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPass};Pooling=true;";

// Build Npgsql data source with dynamic JSON support for List<string> jsonb columns
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.EnableDynamicJson();
var dataSource = dataSourceBuilder.Build();

// Configure DbContext with the connection string
builder.Services.AddDbContext<MasterdataDbContext>(options =>
    options.UseNpgsql(dataSource,
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "masterdata");
        }));

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

// Configure Authorization - open access, no token required
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .RequireAssertion(_ => true) // Allow all requests regardless of auth state
        .Build();
});

// Register DbContext
builder.Services.AddDbContext<MasterdataDbContext>();

// Register HTTP context and memory cache first as they're used by other services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITokenExtractionService, TokenExtractionService>();

// Base repositories
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

// Entity repositories (alphabetical order for better maintainability)
builder.Services.AddScoped<IRepository<Affiliation>, Repository<Affiliation>>();
builder.Services.AddScoped<IRepository<AxleConfiguration>, Repository<AxleConfiguration>>();
builder.Services.AddScoped<IRepository<Customer>, Repository<Customer>>();
builder.Services.AddScoped<IRepository<Driver>, Repository<Driver>>();
builder.Services.AddScoped<IRepository<DriverVehicle>, Repository<DriverVehicle>>();
builder.Services.AddScoped<IRepository<Organisation>, Repository<Organisation>>();
builder.Services.AddScoped<IRepository<Owner>, Repository<Owner>>();
builder.Services.AddScoped<IRepository<Product>, Repository<Product>>();
builder.Services.AddScoped<IRepository<Masterdata.Core.Entities.Route>, Repository<Masterdata.Core.Entities.Route>>();
builder.Services.AddScoped<IRepository<Sacco>, Repository<Sacco>>();
builder.Services.AddScoped<IRepository<Supplier>, Repository<Supplier>>();
builder.Services.AddScoped<IRepository<Transporter>, Repository<Transporter>>();
builder.Services.AddScoped<IRepository<Vehicle>, Repository<Vehicle>>();
builder.Services.AddScoped<IRepository<Weighbridge>, Repository<Weighbridge>>();

// Note: BaseEntity is an abstract class and should not be registered as a repository
// Note: MainEntity.Base is likely a base class and should not be registered directly


// Register services (alphabetical order for better maintainability)
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IDriverService, DriverService>();
builder.Services.AddScoped<IOrganisationService, OrganisationService>();
builder.Services.AddScoped<IOwnerService, OwnerService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IRouteService, RouteService>();
builder.Services.AddScoped<ISaccoService, SaccoService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<ITransporterService, TransporterService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IWeighbridgeService, WeighbridgeService>();
builder.Services.AddScoped<IAxleConfigurationService, AxleConfigurationService>();
// Note: IDriverVehicleService is not implemented yet

// Add CORS with more permissive settings for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .SetIsOriginAllowed(origin => true) // Allow any origin
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Important for cookies, authorization headers with HTTPS
    });
});

// Add Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Enable CORS before other middleware
app.UseCors("AllowAll");

// Configure the HTTP request pipeline
// ALWAYS generate Swagger JSON (for both DEV + PROD)
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
        c.SwaggerEndpoint("v1/swagger.json", "Masterdata API V1");
        c.RoutePrefix = "swagger";  // ← FIXED: Serve UI at /swagger
        c.DocumentTitle = "QaliTrack Masterdata API (Read-Only)";
    });
}
app.UseHttpsRedirection();
// Use Serilog request logging
app.UseSerilogRequestLogging();

app.UseRouting();
app.UseHttpMetrics();

// CORS must be after UseRouting() but before UseAuthentication() and UseAuthorization()
app.UseCors("AllowAll");

// Add global exception handler
app.UseGlobalExceptionHandler();

// Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapMetrics();

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