using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using Serilog;
using System.Text;
using HealthChecks.UI.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using QaliTrack.Gateway.Services;
using MMLib.SwaggerForOcelot.DependencyInjection;
using MMLib.SwaggerForOcelot.Middleware;
using QaliTrackGateway.Extensions;
using QaliTrackGateway.Services;
using QaliTrackGateway.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/gateway-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add memory cache for role authorization
builder.Services.AddMemoryCache();

// Register authorization cache service
builder.Services.AddSingleton<QaliTrackGateway.Services.IAuthorizationCacheService, QaliTrackGateway.Services.AuthorizationCacheService>();

// Configure audit settings
builder.Services.Configure<AuditSettings>(builder.Configuration.GetSection(AuditSettings.SectionName));

// Register audit service with configured HTTP client
builder.Services.AddHttpClient<IAuditService, AuditService>((serviceProvider, client) =>
{
    var auditSettings = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AuditSettings>>().Value;
    client.BaseAddress = new Uri(auditSettings.TransactionServiceBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(auditSettings.HttpTimeoutSeconds);
    client.DefaultRequestHeaders.Add("User-Agent", "QaliTrack-Gateway/1.0");
});

builder.Services.AddSingleton<IAuditService, AuditService>();

// Configure monitoring settings
builder.Services.Configure<MonitoringSettings>(builder.Configuration.GetSection(MonitoringSettings.SectionName));

// Register service health monitor
builder.Services.AddHttpClient<IServiceHealthMonitor, ServiceHealthMonitor>((serviceProvider, client) =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "QaliTrack-Gateway-Monitor/1.0");
});

builder.Services.AddSingleton<IServiceHealthMonitor, ServiceHealthMonitor>();

// Register health-aware routing service
builder.Services.AddSingleton<IHealthAwareRoutingService, HealthAwareRoutingService>();

// Add HTTP client factory and configuration service
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "QaliTrack API Gateway", 
        Version = "v1",
        Description = "QaliTrack Microservices API Gateway - Central entry point for all services"
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

// Add JWT Authentication - Support both mock and real services
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
var issuer = jwtSettings["Issuer"] ?? "UserService";
var audience = jwtSettings["Audience"] ?? "UserService";
var key = Encoding.UTF8.GetBytes(secretKey);

// Check if using mock services
var useMockServices = builder.Configuration.GetValue<bool>("USE_MOCK_SERVICES");
if (useMockServices)
{
    Console.WriteLine("🧪 Using MOCK services for authentication");
    Console.WriteLine($"   JWT Issuer: {issuer}");
    Console.WriteLine($"   JWT Audience: {audience}");
}
else
{
    Console.WriteLine("🔐 Using REAL services for authentication");
    Console.WriteLine($"   JWT Issuer: {issuer}");
    Console.WriteLine($"   JWT Audience: {audience}");
}

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
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Add Health Checks (dynamic based on enabled services)
var healthChecksBuilder = builder.Services.AddHealthChecks();

// Get enabled services dynamically from client configuration or environment
var clientCode = builder.Configuration["CLIENT_CODE"] ?? "testing";
var configPath = Path.Combine("configs", "clients", $"{clientCode}.yml");

Console.WriteLine($"Looking for config at: {configPath}");
Console.WriteLine($"Config exists: {File.Exists(configPath)}");

if (File.Exists(configPath))
{
    try
    {
        var deserializer = new YamlDotNet.Serialization.Deserializer();
        var configContent = File.ReadAllText(configPath);
        Console.WriteLine($"Config content loaded, length: {configContent.Length}");
        
        var config = deserializer.Deserialize<Dictionary<string, object>>(configContent);
        Console.WriteLine($"Config parsed successfully: {config != null}");
        
        // Add health checks for enabled services
        if (config.ContainsKey("services") && config["services"] is Dictionary<object, object> servicesDict)
        {
            Console.WriteLine($"Found services in config, count: {servicesDict.Count}");
            foreach (var service in servicesDict)
            {
                var serviceName = service.Key.ToString();
                
                if (service.Value is Dictionary<object, object> serviceConfig)
                {
                    bool isEnabled = serviceConfig.ContainsKey("enabled") && 
                                   serviceConfig["enabled"].ToString().ToLower() == "true";
                    
                    if (isEnabled && serviceName != "gateway")
                    {
                        var healthUrl = $"http://{serviceName}/health";
                        healthChecksBuilder.AddUrlGroup(new Uri(healthUrl), serviceName, HealthStatus.Degraded);
                        Console.WriteLine($"Added health check for {serviceName} at {healthUrl}");
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        // Fallback to hardcoded services for development
        healthChecksBuilder
            .AddUrlGroup(new Uri("http://user-service/health"), "user-service", HealthStatus.Degraded)
            .AddUrlGroup(new Uri("http://customer-service/health"), "customer-service", HealthStatus.Degraded)
            .AddUrlGroup(new Uri("http://swagger-aggregator/health"), "swagger-aggregator", HealthStatus.Degraded)
            .AddUrlGroup(new Uri("http://product-service/health"), "product-service", HealthStatus.Degraded);
    }
}
else if (builder.Environment.IsDevelopment())
{
    // Fallback for development when config file is not available
    healthChecksBuilder
        .AddUrlGroup(new Uri("http://user-service/health"), "user-service", HealthStatus.Degraded)
        .AddUrlGroup(new Uri("http://customer-service/health"), "customer-service", HealthStatus.Degraded)
        .AddUrlGroup(new Uri("http://swagger-aggregator/health"), "swagger-aggregator", HealthStatus.Degraded)
        .AddUrlGroup(new Uri("http://product-service/health"), "product-service", HealthStatus.Degraded);
}
else
{
    // In production, check all services
    healthChecksBuilder
        .AddUrlGroup(new Uri("https://localhost:7001/health"), "user-service", HealthStatus.Degraded)
        .AddUrlGroup(new Uri("https://localhost:7002/health"), "organization-service", HealthStatus.Degraded)
        .AddUrlGroup(new Uri("https://localhost:7003/health"), "vehicle-service", HealthStatus.Degraded)
        .AddUrlGroup(new Uri("https://localhost:7004/health"), "driver-service", HealthStatus.Degraded)
        .AddUrlGroup(new Uri("https://localhost:7005/health"), "product-service", HealthStatus.Degraded)
        .AddUrlGroup(new Uri("https://localhost:7006/health"), "route-service", HealthStatus.Degraded)
        .AddUrlGroup(new Uri("https://localhost:7007/health"), "weighbridge-service", HealthStatus.Degraded)
        .AddUrlGroup(new Uri("https://localhost:7008/health"), "customer-service", HealthStatus.Degraded)
        .AddUrlGroup(new Uri("https://localhost:7009/health"), "supplier-service", HealthStatus.Degraded)
        .AddUrlGroup(new Uri("https://localhost:7010/health"), "transporter-service", HealthStatus.Degraded)
        .AddUrlGroup(new Uri("https://localhost:7011/health"), "sacco-service", HealthStatus.Degraded)
        .AddUrlGroup(new Uri("https://localhost:7012/health"), "weight-data-service", HealthStatus.Degraded)
        .AddUrlGroup(new Uri("https://localhost:7013/health"), "compliance-service", HealthStatus.Degraded)
        .AddUrlGroup(new Uri("https://localhost:7014/health"), "operational-data-service", HealthStatus.Degraded)
        .AddUrlGroup(new Uri("https://localhost:7015/health"), "transaction-service", HealthStatus.Degraded)
        .AddUrlGroup(new Uri("https://localhost:7016/health"), "analytics-service", HealthStatus.Degraded)
        .AddUrlGroup(new Uri("https://localhost:7017/health"), "data-sync-service", HealthStatus.Degraded)
        .AddUrlGroup(new Uri("https://localhost:7018/health"), "archive-service", HealthStatus.Degraded);
}

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
    
    options.AddPolicy("AllowSwagger", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("*");
    });
});

// Add Ocelot configuration based on environment
if (builder.Environment.EnvironmentName == "Testing")
{
    // For testing environment, use in-memory configuration provided by tests
    // Do not load file-based Ocelot configurations to avoid route duplicates
}
else if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("ocelot.development.json", optional: false, reloadOnChange: false);
    builder.Configuration.AddJsonFile("ocelot.SwaggerEndPoints.json", optional: false, reloadOnChange: false);
}
else
{
    builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: false);
    builder.Configuration.AddJsonFile("ocelot.SwaggerEndPoints.json", optional: false, reloadOnChange: false);
}
builder.Services.AddOcelot().AddConsul();
builder.Services.AddSwaggerForOcelot(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseCors("AllowAll");
app.UseHttpsRedirection();

// Serve static files including documentation
app.UseStaticFiles();

// Configure documentation serving
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "docs")),
    RequestPath = "/docs"
});

// Documentation default route
app.MapGet("/docs", () => Results.Redirect("/docs/index.html"));
app.MapFallback("/docs/{**path}", async context =>
{
    var path = context.Request.Path.Value?.Replace("/docs/", "") ?? "index.html";
    if (string.IsNullOrEmpty(path) || path == "/")
        path = "index.html";
    
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "docs", path);
    if (File.Exists(filePath))
    {
        // Set proper Content-Type based on file extension
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var contentType = extension switch
        {
            ".html" => "text/html; charset=utf-8",
            ".css" => "text/css; charset=utf-8",
            ".js" => "application/javascript; charset=utf-8",
            ".json" => "application/json; charset=utf-8",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".svg" => "image/svg+xml",
            ".ico" => "image/x-icon",
            ".yml" or ".yaml" => "text/yaml; charset=utf-8",
            _ => "application/octet-stream"
        };
        
        context.Response.ContentType = contentType;
        await context.Response.SendFileAsync(filePath);
    }
    else
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("Documentation file not found");
    }
});

if (app.Environment.IsDevelopment())
{
    // Add middleware to handle CORS for Swagger
    app.Use(async (context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            if (context.Request.Headers.ContainsKey("Origin"))
            {
                context.Response.Headers["Access-Control-Allow-Origin"] = "*";
                context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS";
                context.Response.Headers["Access-Control-Allow-Headers"] = "*";
            }
        }
        await next();
    });
    
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "QaliTrack API Gateway V1");
        c.RoutePrefix = "swagger";
    });
}

// Use Serilog request logging
app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

// Add role-based authorization middleware
app.UseRoleAuthorization();

// Add health check middleware before Ocelot
app.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions()
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.UseHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions()
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.UseHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions()
{
    Predicate = _ => false,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

Log.Information("QaliTrack API Gateway starting up...");

// Start service health monitoring
var healthMonitor = app.Services.GetRequiredService<IServiceHealthMonitor>();
await healthMonitor.StartMonitoringAsync();

// Map controllers for Gateway's own endpoints
app.MapControllers();

// Use Ocelot middleware - it will handle routing based on configuration
await app.UseOcelot();

app.Run();

// Make Program class accessible for testing
public partial class Program { }