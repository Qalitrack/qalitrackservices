using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using UserService.Core.ServiceRegistration;
using UserService.Infrastructure.ServiceRegistration;
using UserService.Infrastructure.Data;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using UserService.Api.Authorization;
using UserService.Core.Mappings;
using System.Security.Claims;
using UserService.Core.Services;

var builder = WebApplication.CreateBuilder(args);

try
{
    // Configure logging
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .WriteTo.Console()
        .CreateLogger();
    builder.Host.UseSerilog();
    Log.Information("Starting UserService application...");

    // Configure services
    ConfigureServices(builder);
    
    var app = builder.Build();
    
    // Configure middleware pipeline
    ConfigurePipeline(app);
    
    Log.Information("UserService application started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

static void ConfigureServices(WebApplicationBuilder builder)
{
    var services = builder.Services;
    
    // Configure MVC
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    
    // Configure API versioning
    services.AddApiVersioning(options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
        options.ApiVersionReader = new Asp.Versioning.UrlSegmentApiVersionReader();
    }).AddMvc();
    
    // Register application services
    services.AddCoreServices();
    services.AddInfrastructureServices(builder.Configuration);
    services.AddAutoMapper(typeof(UserProfile));
    services.AddOutputCache();
    
    // Register background services
    services.AddHostedService<ShiftInstanceBackgroundService>();
    // Configure PostgreSQL
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Database connection string is not configured.");
    services.AddDbContext<UserServiceDbContext>(options =>
    {
        options.UseNpgsql(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly("UserService.Infrastructure");
            sqlOptions.CommandTimeout(15);
            sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
        });
    });
    // Configure authorization
    services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
    services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
    services.AddAuthorization(options =>
    {
        options.DefaultPolicy = new AuthorizationPolicyBuilder()
            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .Build();
        options.AddPolicy("RequireAdminRole", policy => 
            policy.RequireRole("Admin")
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme));
    });
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

services.AddAuthentication(options =>
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
  
    // Configure CORS
    services.AddCors(options =>
    {
        options.AddPolicy("RestrictedCors", policy =>
        {
            policy.SetIsOriginAllowed(origin =>
                    origin.StartsWith("http://localhost") || origin.StartsWith("http://127.0.0.1"))
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });
    
    // Configure Swagger
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo 
        { 
            Title = "UserService API", 
            Version = "v1"
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

        // Apply the security requirement globally or to specific endpoints
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
}

static void ConfigurePipeline(WebApplication app)
{
    app.UseSerilogRequestLogging();
    app.UseRouting();
    
    // Debug middleware (remove in production)
    if (app.Environment.IsDevelopment())
    {
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
    
    app.UseCors("RestrictedCors");
    app.UseAuthentication(); // This must come before Authorization
    app.UseAuthorization();
    app.UseSwagger();
    app.UseOutputCache();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserService API V1");
        c.RoutePrefix = string.Empty;
    });
    
    app.MapControllers();
    app.MapHealthChecks("/health");
    
    // Initialize database
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<UserServiceDbContext>();
    context.Database.Migrate();
    PrepDb.PrepPopulation(app, isProduction: false);
}

