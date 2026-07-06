using Microsoft.AspNetCore.Authentication.JwtBearer;
using Prometheus;
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
    // Configure Serilog
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .CreateLogger();

    builder.Host.UseSerilog();
    Log.Information("Starting UserService application...");

    // ==================== SERVICES ====================
    var services = builder.Services;

    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddOutputCache();
    services.AddHttpContextAccessor();

    // API Versioning
    services.AddApiVersioning(options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
        options.ApiVersionReader = new Asp.Versioning.UrlSegmentApiVersionReader();
    }).AddMvc();

    // Register Core + Infrastructure + AutoMapper
    services.AddCoreServices();
    services.AddInfrastructureServices(builder.Configuration);
    services.AddAutoMapper(cfg => cfg.AddMaps(typeof(UserProfile).Assembly));

    // PostgreSQL
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Database connection string is not configured.");

    services.AddDbContext<UserServiceDbContext>(options =>
    {
        options.UseNpgsql(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly("UserService.Infrastructure");
            sqlOptions.CommandTimeout(15);
            sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
            sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "users");
        });
    });

    // Authorization
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

    // JWT Configuration
    var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                    ?? builder.Configuration["JwtSettings:SecretKey"]
                    ?? throw new InvalidOperationException("JWT Secret Key is not configured.");

    var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                 ?? builder.Configuration["JwtSettings:Issuer"]
                 ?? "UserService";

    var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                   ?? builder.Configuration["JwtSettings:Audience"]
                   ?? "UserService";

    if (secretKey.Length < 32)
        throw new InvalidOperationException("JWT Secret Key must be at least 32 characters long.");

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

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

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Warning("JWT Authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Debug("JWT Token validated for user: {User}", context.Principal?.Identity?.Name);
                return Task.CompletedTask;
            }
        };
    });

    // CORS
    services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });

    // Health Checks
    services.AddHealthChecks();

    // ==================== SWAGGER (NOW IDENTICAL TO TECHNICIAN) ====================
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "UserService API",
            Version = "v1",
            Description = "QaliTrack UserService - Authentication, Users, Roles & Permissions"
        });

        // JWT Bearer Auth Definition
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

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
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    // ==================== PIPELINE (MATCHES TECHNICIAN EXACTLY) ====================

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseHttpMetrics();

    // CORS
    app.UseCors("AllowAll");

    // Always generate Swagger JSON (even in Production)
    app.UseSwagger();

    // Swagger UI: Different behavior per environment (like TechnicianApi)
    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserService API V1");
            c.RoutePrefix = string.Empty; // Root → http://localhost:5000/
            c.DocumentTitle = "QaliTrack - UserService API (Dev)";
        });
    }
    else
    {
        // PRODUCTION: Swagger UI at /swagger → https://qalitrack.cseco.co.ke/tech/user/swagger
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("v1/swagger.json", "UserService API V1"); // Relative path!
            c.RoutePrefix = "swagger";
            c.DocumentTitle = "QaliTrack UserService API (Read-Only)";
        });
    }

    // DocFX Static Documentation
    var docfxPath = app.Environment.IsDevelopment()
        ? Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "_site"))
        : Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "docs");

    if (Directory.Exists(docfxPath))
    {
        app.UseFileServer(new FileServerOptions
        {
            FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(docfxPath),
            RequestPath = "/docs",
            EnableDirectoryBrowsing = false
        });

        // Download documentation as PDF
        app.MapGet("/docs/download", () =>
        {
            var pdfPath = Path.Combine(docfxPath, "UserService-Docs.pdf");
            if (!File.Exists(pdfPath))
                return Results.NotFound("Documentation PDF not yet generated.");
            return Results.File(pdfPath, "application/pdf", "UserService-Docs.pdf");
        }).AllowAnonymous();
    }

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseOutputCache();

    app.MapControllers();
    app.MapHealthChecks("/health");
    app.MapMetrics();

    // Database Migration (with proper logging)
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<UserServiceDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Applying database migrations for UserService...");
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");

            await PrepDb.PrepPopulation(app, isProduction: app.Environment.IsProduction());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the UserService database.");
            throw;
        }
    }

    Log.Information("UserService started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "UserService terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// For testing
public partial class Program { }