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
using UserService.Core.Services;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Repositories;

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
    builder.WebHost.UseUrls("http://localhost:8080");

    // Add services to the container
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = null; // Preserve property casing
        });
    
    builder.Services.AddEndpointsApiExplorer();
    
    // Register application services
    RegisterServices(builder.Services);
    
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
    builder.Services.AddHealthChecks();
    
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

public partial class Program
{
    private static void RegisterServices(IServiceCollection services)
    {
        // Core Services
        services.AddScoped<IUserService, UserService.Core.Services.UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<IUserRoleService, UserRoleService>();
        services.AddScoped<IPermissionsService, PermissionsService>();
        services.AddScoped<ITokenService, TokenService>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IShiftRepository, ShiftRepository>();
        services.AddScoped<IUserShiftRepository, UserShiftRepository>();
        services.AddScoped<ITokenRepository, TokenRepository>();
        services.AddScoped<IPermissionsRepository, PermissionsRepository>();
        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>(); // ✅ THIS LINE WAS MISSING

        // Infrastructure
        services.AddHttpContextAccessor();
        services.AddAutoMapper(typeof(UserProfile));

        Log.Information("Application services registered.");
    }

    private static void ConfigureDatabase(WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<UserServiceDbContext>(options =>
            options.UseSqlite(
                builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                "Data Source=user-service.db",
                b => b.MigrationsAssembly("UserService.Infrastructure")));
                
        Log.Information("Database configured.");
    }

   private static void ConfigureJwtAuthentication(WebApplicationBuilder builder)
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
            // Use JWT standard claim types
            NameClaimType = "sub",
            RoleClaimType = "role"
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Get the token from the Authorization header
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                
                // If token is not in the header, try to get it from the query string (for WebSocket connections)
                if (string.IsNullOrEmpty(token) && context.Request.Query.TryGetValue("access_token", out var tokenValues))
                {
                    token = tokenValues.FirstOrDefault();
                }

                if (!string.IsNullOrEmpty(token))
                {
                    // Ensure the token has the "Bearer " prefix if it's not there
                    if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) && 
                        !token.Contains(' '))
                    {
                        token = "Bearer " + token;
                        context.Request.Headers["Authorization"] = token;
                    }
                    
                    // Log the token (without logging the actual token value in production)
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
                    // Ensure we have a name claim
                    var sub = identity.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
                    if (sub != null)
                    {
                        // Map 'sub' to multiple claim types for compatibility
                        if (!identity.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
                            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, sub));
                            
                        if (!identity.HasClaim(c => c.Type == ClaimTypes.Name))
                            identity.AddClaim(new Claim(ClaimTypes.Name, sub));
                    }

                    // Log all claims for debugging
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

    private static void ConfigureAuthorization(IServiceCollection services)
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

    private static void ConfigureSwagger(IServiceCollection services)
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

    private static void ConfigureCors(IServiceCollection services)
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

    private static void ConfigureMiddleware(WebApplication app)
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

    private static async Task InitializeDatabaseAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        
        try
        {
            var context = services.GetRequiredService<UserServiceDbContext>();
            await context.Database.MigrateAsync();
            await PrepDb.PrepPopulation(app, app.Environment.IsProduction());
            Log.Information("Database initialized successfully.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while initializing the database");
            throw;
        }
    }
}