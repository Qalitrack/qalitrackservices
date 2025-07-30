using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using UserService.Api.Authorization;
using UserService.Core.Interfaces;
using UserService.Core.Mappings;
using UserService.Core.Options;
using UserService.Core.Services;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Interfaces;
using UserService.Infrastructure.Repositories;
using UserService.Infrastructure.Services;
using UserService.Api.Middleware;
using UserService.Api.Filters;
var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/user-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting application...");

    // Configure to listen on port 8080
  //  builder.WebHost.UseUrls("http://localhost:8081");

    // Add services to the container
    builder.Services.AddControllers(options =>
        {
            options.Filters.Add<ModelValidationFilter>();
        })
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = null; // Preserve property casing
        });
    
    builder.Services.AddEndpointsApiExplorer();
    
    // Add API Versioning
    builder.Services.AddApiVersioning(options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
        options.ApiVersionReader = Asp.Versioning.ApiVersionReader.Combine(
            new Asp.Versioning.UrlSegmentApiVersionReader(),
            new Asp.Versioning.HeaderApiVersionReader("X-Version"),
            new Asp.Versioning.QueryStringApiVersionReader("version")
        );
    }).AddMvc().AddApiExplorer(setup =>
    {
        setup.GroupNameFormat = "'v'VVV";
        setup.SubstituteApiVersionInUrl = true;
    });
    
    // Register application services
    RegisterServices(builder.Services, builder);
    
    // Configure database
    ConfigureDatabase(builder);
    
    // Configure JWT Authentication
    ConfigureJwtAuthentication(builder);
    
    // Configure Authorization
    ConfigureAuthorization(builder.Services);
    
    // Configure Swagger
    ConfigureSwagger(builder.Services);
    
    // Configure CORS
    ConfigureCors(builder.Services);
    
    // Add health checks
    // builder.Services.AddHealthChecks()
    //     .AddCheck<BackupHealthCheck>("backup");
    
    var app = builder.Build();
    
    // Configure the HTTP request pipeline
    ConfigureMiddleware(app);
    
    // Run migrations and seed database
    await InitializeDatabaseAsync(app);
    
    Log.Information("Application is now running...");
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

static void RegisterServices(IServiceCollection services, WebApplicationBuilder builder)
{  
    
    // Core Services - Register base service first, then decorate with caching
    services.AddScoped<UserService.Core.Services.UserService>();
    services.AddScoped<IUserService>(provider =>
    {
        var baseUserService = provider.GetRequiredService<UserService.Core.Services.UserService>();
        var cacheService = provider.GetRequiredService<ICacheService>();
        return new CachedUserService(baseUserService, cacheService);
    });
    services.AddScoped<IRoleService, RoleService>();
    services.AddScoped<IShiftService, ShiftService>();
    services.AddScoped<IUserRoleService, UserRoleService>();
    services.AddScoped<IPermissionsService, PermissionsService>();
    services.AddScoped<ITokenService, TokenService>();
    services.AddScoped<IUserStatusService, UserStatusService>();
    services.AddScoped<IReportService, ReportService>();
    services.AddScoped<ITwoFactorService, TwoFactorService>();
    
    // Email queue services for improved performance
    services.AddSingleton<EmailQueueService>();
    services.AddScoped<IEmailQueueService>(provider => provider.GetRequiredService<EmailQueueService>());
    
    // JWT Configuration Service
    services.AddScoped<IJwtConfigurationService, JwtConfigurationService>();

    // Repositories
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IRoleRepository, RoleRepository>();
    services.AddScoped<IShiftRepository, ShiftRepository>();
    services.AddScoped<IUserShiftRepository, UserShiftRepository>();
    services.AddScoped<ITokenRepository, TokenRepository>();
    services.AddScoped<IPermissionsRepository, PermissionsRepository>();
    services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
    services.AddScoped<IUserStatusRepository, UserStatusRepository>();
    services.AddScoped<IUserRoleRepository, UserRoleRepository>();

    // Infrastructure
    services.AddHttpContextAccessor();
    services.AddAutoMapper(typeof(UserProfile));
    services.AddHealthChecks(); // This registers all necessary health check services
    
    // Configure caching for 1000+ users
    var useRedis = builder.Configuration.GetValue<bool>("UseRedis", false);
    var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
    
    if (useRedis && !string.IsNullOrEmpty(redisConnectionString))
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "UserService";
        });
        Log.Information("Redis distributed cache configured for 1000+ users");
    }
    else
    {
        services.AddMemoryCache();
        Log.Information("Memory cache configured (suitable for <500 users)");
    }
    
    // Register BackupOptions from configuration
    services.Configure<BackupOptions>(builder.Configuration.GetSection(BackupOptions.SectionName));
    
    // Register our health check service
    services.AddScoped<IHealthCheckService, HealthCheckService>();
    
    // Register cache service based on configuration
    if (useRedis && !string.IsNullOrEmpty(redisConnectionString))
    {
        services.AddScoped<ICacheService, RedisCacheService>();
    }
    else
    {
        services.AddScoped<ICacheService, MemoryCacheService>();
    }
    
    // Backup Services
    // services.Configure<BackupOptions>(builder.Configuration.GetSection(BackupOptions.SectionName));
    // services.AddScoped<IDatabaseBackupService, DatabaseBackupService>();
    // services.AddScoped<RestoreService>();
    // services.AddScoped<IBackupNotificationService, BackupNotificationService>();
    // services.AddScoped<IBackupVerificationService, BackupVerificationService>();
    //
    // // Background  Services
    // services.AddHostedService<BackupScheduler>();
    // services.AddHostedService<BackupMonitor>();
    services.AddHostedService<ShiftMonitorService>();
    services.AddHostedService<EmailProcessorService>();
        
    // Register SMTP Email Service for 2FA
    services.AddScoped<IEmailService, SmtpEmailService>();

    Log.Information("Application services registered.");
}

static void ConfigureDatabase(WebApplicationBuilder builder)
{
    // Ensure backup directory exists
    var backupOptions = builder.Configuration.GetSection(BackupOptions.SectionName).Get<BackupOptions>();
    if (backupOptions?.Path != null && !Directory.Exists(backupOptions.Path))
    {
        try
        {
            Directory.CreateDirectory(backupOptions.Path);
            builder.Logging.AddConsole().Services.BuildServiceProvider()
                .GetRequiredService<ILogger<Program>>()
                .LogInformation("Created backup directory: {BackupPath}", 
                    Path.GetFullPath(backupOptions.Path));
        }
        catch (Exception ex)
        {
            builder.Logging.AddConsole().Services.BuildServiceProvider()
                .GetRequiredService<ILogger<Program>>()
                .LogError(ex, "Failed to create backup directory: {BackupPath}", 
                    backupOptions.Path);
            throw;
        }
    }

    // Configure database - PostgreSQL for production, SQLite for development
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var usePostgreSQL = builder.Configuration.GetValue<bool>("UsePostgreSQL", false);
    
    builder.Services.AddDbContext<UserServiceDbContext>(options =>
    {
        if (usePostgreSQL && !string.IsNullOrEmpty(connectionString))
        {
            options.UseNpgsql(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly("UserService.Infrastructure");
                sqlOptions.CommandTimeout(15); // Reduced for better performance
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
            });
        }
        else
        {
            options.UseSqlite(
                connectionString ?? "Data Source=user-service.db",
                sqlOptions => 
                {
                    sqlOptions.MigrationsAssembly("UserService.Infrastructure");
                    sqlOptions.CommandTimeout(15);
                });
        }
        
        // Performance optimizations for 1000+ users
        options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    });
            
    Log.Information("Database configured.");
}

static void ConfigureJwtAuthentication(WebApplicationBuilder builder)
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    
    // Priority: Environment variables > appsettings.json
    var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ??
                   jwtSettings["SecretKey"] ?? 
                   builder.Configuration["Jwt:SecretKey"] ??
                   throw new InvalidOperationException("JWT Secret Key is not configured. Set JWT_SECRET_KEY environment variable or Jwt:SecretKey in configuration.");
    
    var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ??
                jwtSettings["Issuer"] ?? 
                builder.Configuration["Jwt:Issuer"] ?? 
                "UserService";
    
    var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ??
                  jwtSettings["Audience"] ?? 
                  builder.Configuration["Jwt:Audience"] ?? 
                  "UserService";

    // Validate secret key security
    if (secretKey.Length < 32)
    {
        throw new InvalidOperationException("JWT Secret Key must be at least 32 characters long for security.");
    }
    
    if (secretKey == "your-super-secret-key-here-minimum-32-characters" || 
        secretKey == "YourSuperSecretJwtSigningKeyThatMustBeAtLeast32CharactersLong!")
    {
        throw new InvalidOperationException("Default JWT Secret Key detected. Please set a secure key in configuration or JWT_SECRET_KEY environment variable.");
    }

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

    // Clear default claim type mappings to prevent conflicts
    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
    JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // Set to true in production
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
            NameClaimType = "sub",
            RoleClaimType = "role"
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                
                if (string.IsNullOrEmpty(token) && context.Request.Query.TryGetValue("access_token", out var tokenValues))
                {
                    token = tokenValues.FirstOrDefault();
                }

                if (!string.IsNullOrEmpty(token))
                {
                    if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) && 
                        !token.Contains(' '))
                    {
                        token = "Bearer " + token;
                        context.Request.Headers["Authorization"] = token;
                    }
                    
                    var tokenPreview = token.Length > 10 
                        ? token.Substring(0, 10) + "..." 
                        : "[invalid]";
                    Log.Debug("JWT Token found: {TokenPreview}", tokenPreview);
                }
                else
                {
                    Log.Debug("No JWT token found in the request");
                }
                
                return Task.CompletedTask;
            },
            
            OnTokenValidated = context =>
            {
                var identity = context.Principal?.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    var sub = identity.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
                    if (sub != null)
                    {
                        if (!identity.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
                            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, sub));
                            
                        if (!identity.HasClaim(c => c.Type == ClaimTypes.Name))
                            identity.AddClaim(new Claim(ClaimTypes.Name, sub));
                    }

                    Log.Debug("User claims after validation: {Claims}",
                        string.Join(", ", identity.Claims.Select(c => $"{c.Type}={c.Value}")));
                }

                var userId = context.Principal?.FindFirst("sub")?.Value;
                Log.Information("JWT Token validated for user: {UserId}", userId);
                
                return Task.CompletedTask;
            },
            
            OnAuthenticationFailed = context =>
            {
                Log.Error(context.Exception, "JWT Authentication failed");
                
                if (context.Exception is SecurityTokenExpiredException)
                {
                    Log.Warning("Token has expired");
                    context.Response.Headers.Add("Token-Expired", "true");
                }
                else if (context.Exception is SecurityTokenInvalidSignatureException)
                {
                    Log.Error("Token signature validation failed. This could be due to an invalid secret key.");
                }
                else if (context.Exception is SecurityTokenNoExpirationException)
                {
                    Log.Error("Token has no expiration");
                }
                
                return Task.CompletedTask;
            },
            
            OnForbidden = context =>
            {
                var userId = context.Principal?.FindFirst("sub")?.Value ?? "unknown";
                Log.Warning("Forbidden: User {UserId} is authenticated but lacks required permissions", userId);
                return Task.CompletedTask;
            }
        };
    });
    
    Log.Information("JWT Authentication configured. Issuer: {Issuer}, Audience: {Audience}", issuer, audience);
}


static void ConfigureAuthorization(IServiceCollection services)
{
    services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
    services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
    
    services.AddAuthorization(options =>
    {
        // Add default policy that requires authentication
        options.DefaultPolicy = new AuthorizationPolicyBuilder()
            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .Build();
            
        // Add a policy that requires the user to be an admin
        options.AddPolicy("RequireAdminRole", policy => 
            policy.RequireRole("Admin")
                  .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme));
    });
    
    Log.Information("Authorization configured.");
}

static void ConfigureSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo 
        { 
            Title = "UserService API", 
            Version = "v1",
            Description = "UserService API",
            Contact = new OpenApiContact
            {
                Name = "Support",
                Email = "support@example.com"
            }
        });
        
        // Add JWT Authentication to Swagger
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme",
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
        
        // Enable XML comments if available
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
    });
    
    Log.Information("Swagger configured.");
}

static void ConfigureCors(IServiceCollection services)
{
    services.AddCors(options =>
    {
        options.AddPolicy("RestrictedCors", policy =>
        {
            policy.WithOrigins("https://localhost:3000", "https://localhost:3001", "https://yourdomain.com")
                  .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
                  .WithHeaders("Content-Type", "Authorization", "X-Requested-With")
                  .AllowCredentials();
        });
        
        // Development policy for local testing
        options.AddPolicy("DevelopmentCors", policy =>
        {
            policy.WithOrigins("http://localhost:3000", "https://localhost:3000", 
                              "http://localhost:3001", "https://localhost:3001")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });
    
    Log.Information("CORS configured with restricted origins.");
}

static void ConfigureMiddleware(WebApplication app)
{
    // Add global exception handling middleware first
    app.UseMiddleware<GlobalExceptionMiddleware>();
    
    // Add validation middleware
    app.UseMiddleware<ValidationMiddleware>();
    
    // Add rate limiting middleware - Redis-based for 1000+ users
    var useRedisRateLimit = app.Configuration.GetValue<bool>("UseRedis", false);
    if (useRedisRateLimit)
    {
        app.UseMiddleware<RedisRateLimitMiddleware>();
    }
    else
    {
        app.UseMiddleware<RateLimitMiddleware>();
    }
    
    // Enable Swagger in all environments for testing containers
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserService API V1");
        c.RoutePrefix = "swagger";
    });
    
    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }
    
    
    app.UseSerilogRequestLogging();
    app.UseRouting();
    
    // CORS must come before UseAuthentication and UseAuthorization
    var corsPolicy = app.Environment.IsDevelopment() ? "DevelopmentCors" : "RestrictedCors";
    app.UseCors(corsPolicy);
    
    // Authentication must come before Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        endpoints.MapHealthChecks("/health");
    });
    
    Log.Information("Middleware pipeline configured.");
}

static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    
    try
    {
        var context = services.GetRequiredService<UserServiceDbContext>();
        await context.Database.MigrateAsync();
        
        // Seed initial data if needed
        await PrepDb.PrepPopulation(app, isProduction: false);        
        Log.Information("Database initialized successfully.");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while initializing the database");
        throw;
    }
}
