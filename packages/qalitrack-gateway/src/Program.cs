using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;
using System.Text;
using HealthChecks.UI.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using QaliTrack.Gateway.Services;
using MMLib.SwaggerForOcelot.DependencyInjection;
using MMLib.SwaggerForOcelot.Middleware;
using QaliTrackGateway.Extensions;

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

// Add Health Checks (conditional based on environment)
var healthChecksBuilder = builder.Services.AddHealthChecks();

if (builder.Environment.IsDevelopment())
{
    // In development, only check services that are actually running
    healthChecksBuilder.AddUrlGroup(new Uri("http://user-service/health"), "user-service", HealthStatus.Degraded);
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
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("ocelot.development.json", optional: false, reloadOnChange: true);
    builder.Configuration.AddJsonFile("ocelot.SwaggerEndPoints.json", optional: false, reloadOnChange: true);
}
else
{
    builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
    builder.Configuration.AddJsonFile("ocelot.SwaggerEndPoints.json", optional: false, reloadOnChange: true);
}
builder.Services.AddOcelot();
builder.Services.AddSwaggerForOcelot(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseCors("AllowAll");
app.UseHttpsRedirection();

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

// Map controllers for Gateway's own endpoints
app.MapControllers();

// Use Ocelot middleware - it will handle routing based on configuration
await app.UseOcelot();

app.Run();

// Make Program class accessible for testing
public partial class Program { }