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
using UserService.Core.Interfaces;

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
    //builder.WebHost.UseUrls("http://localhost:8080");

    // Add services to the container
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = null; // Preserve property casing
        });
    
    builder.Services.AddEndpointsApiExplorer();
    
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
    builder.Services.AddHealthChecks()
        .AddCheck<BackupHealthCheck>("backup");
    
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
    // Core Services
    services.AddScoped<IUserService, UserService.Core.Services.UserService>();
    services.AddScoped<IRoleService, RoleService>();
    services.AddScoped<IShiftService, ShiftService>();
    services.AddScoped<IUserRoleService, UserRoleService>();
    services.AddScoped<IPermissionsService, PermissionsService>();
    services.AddScoped<ITokenService, TokenService>();
    services.AddScoped<IUserStatusService, UserStatusService>();
    services.AddScoped<IReportService, ReportService>();

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
    
    // Register BackupOptions from configuration
    services.Configure<BackupOptions>(builder.Configuration.GetSection(BackupOptions.SectionName));
    
    // Register our health check service
    services.AddScoped<IHealthCheckService, HealthCheckService>();
    
    // Backup Services
    services.Configure<BackupOptions>(builder.Configuration.GetSection(BackupOptions.SectionName));
    services.AddScoped<IDatabaseBackupService, DatabaseBackupService>();
    services.AddScoped< RestoreService>();
    services.AddScoped<IBackupNotificationService, BackupNotificationService>();
    services.AddScoped<IBackupVerificationService, BackupVerificationService>();
    
    // Background Services
    services.AddHostedService<BackupScheduler>();
    services.AddHostedService<BackupMonitor>();
    services.AddHostedService<ShiftMonitorService>();
        
    // Register IEmailService if not already registered
    if (!services.Any(s => s.ServiceType == typeof(IEmailService)))
    {
        services.AddScoped<IEmailService, NullEmailService>();
        var logger = LoggerFactory.Create(logging => 
        {
            logging.AddConsole();
            logging.AddDebug();
        }).CreateLogger<Program>();
        
        logger.LogWarning("No IEmailService implementation found. Using NullEmailService. No emails will be sent.");
    }

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

    builder.Services.AddDbContext<UserServiceDbContext>(options =>
        options.UseSqlite(
            builder.Configuration.GetConnectionString("DefaultConnection") ?? 
            "Data Source=user-service.db",
            sqlOptions => 
            {
                sqlOptions.MigrationsAssembly("UserService.Infrastructure");
                sqlOptions.CommandTimeout(30);
            }));
            
    Log.Information("Database configured.");
}

static void ConfigureJwtAuthentication(WebApplicationBuilder builder)
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"] ?? 
                   builder.Configuration["Jwt:SecretKey"] ??
                   throw new InvalidOperationException("JWT Secret Key is not configured");
    
    var issuer = jwtSettings["Issuer"] ?? 
                builder.Configuration["Jwt:Issuer"] ?? 
                "UserService";
    
    var audience = jwtSettings["Audience"] ?? 
                  builder.Configuration["Jwt:Audience"] ?? 
                  "UserService";

    if (secretKey == "your-super-secret-key-here-minimum-32-characters")
    {
        throw new InvalidOperationException("Default JWT Secret Key detected. Please set a secure key in configuration.");
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
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });
    
    Log.Information("CORS configured.");
}

static void ConfigureMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => 
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserService API V1");
            c.RoutePrefix = "swagger";
        });
    }
    else
    {
        app.UseExceptionHandler("/error");
        app.UseHsts();
    }
    
    
    app.UseSerilogRequestLogging();
    app.UseRouting();
    
    // CORS must come before UseAuthentication and UseAuthorization
    app.UseCors("AllowAll");
    
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
