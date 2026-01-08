using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Yarp.ReverseProxy.Model;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to use HTTP only
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // Use default configuration
    serverOptions.Configure();
});

// Set the HTTP port from environment variable or use default 80
var port = Environment.GetEnvironmentVariable("HTTP_PORT") ?? "80";
builder.WebHost.UseUrls($"http://*:{port}");

// Disable HTTPS redirection
builder.Services.Configure<Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionOptions>(options =>
{
    options.HttpsPort = null;
    options.RedirectStatusCode = (int)HttpStatusCode.TemporaryRedirect;
});

// Disable HTTPS requirement for authentication
builder.Services.Configure<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Authentication:Authority"],
        ValidAudience = builder.Configuration["Authentication:Audience"],
    };
});

// Add Authentication/Authorization
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var cfg = builder.Configuration.GetSection("Authentication");
        options.Authority = cfg["Authority"];
        options.Audience = cfg["Audience"];
        options.RequireHttpsMetadata = false;
        
        // For dev environments that use self-signed certs, you might need to relax HTTPS metadata.
        if (bool.TryParse(cfg["RequireHttpsMetadata"], out var requireHttps))
        {
            options.RequireHttpsMetadata = requireHttps;
        }
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "qalitrack",
            ValidateAudience = true,
            ValidAudience = "qalitrack",
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

// Add Authorization with a default policy that requires authentication
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .Build();
});

// Add YARP Reverse Proxy from configuration
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Configure logging for the application
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();

// Add request logging middleware
// Add request logging for YARP
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    
    // Log the incoming request
    logger.LogInformation($"[YARP] Incoming: {context.Request.Method} {context.Request.Path}{context.Request.QueryString}");
    
    // Get the proxy feature
    var proxyFeature = context.Features.Get<IReverseProxyFeature>();
    if (proxyFeature != null)
    {
        // Log the matched route if available
        if (proxyFeature.Route != null && proxyFeature.Route.Config != null)
        {
            logger.LogInformation($"[YARP] Matched Route: {proxyFeature.Route.Config.RouteId}");
        }
        
        // Log the destination endpoint
        var endpoint = context.GetEndpoint();
        if (endpoint != null)
        {
            logger.LogInformation($"[YARP] Forwarding to: {endpoint.DisplayName}");
        }
    }
    
    await next(context); // Add 'context' parameter here
});

// Add request logging for YARP
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    
    // Log the incoming request
    logger.LogInformation($"[YARP] Incoming: {context.Request.Method} {context.Request.Path}{context.Request.QueryString}");
    
    // Get the proxy feature
    var proxyFeature = context.Features.Get<IReverseProxyFeature>();
    if (proxyFeature != null)
    {
        // Log the matched route if available
        if (proxyFeature.Route != null && proxyFeature.Route.Config != null)
        {
            logger.LogInformation($"[YARP] Matched Route: {proxyFeature.Route.Config.RouteId}");
        }
        
        // Log the destination endpoint
        var endpoint = context.GetEndpoint();
        if (endpoint != null)
        {
            logger.LogInformation($"[YARP] Forwarding to: {endpoint.DisplayName}");
        }
    }
    
    await next();
});

// Configure YARP with default pipeline
app.MapReverseProxy(proxyPipeline =>
{
    proxyPipeline.UseSessionAffinity();
    proxyPipeline.UseLoadBalancing();
    proxyPipeline.UsePassiveHealthChecks();
});

app.UseAuthentication();
app.UseAuthorization();

// Expose a simple health endpoint
app.MapGet("/", () => Results.Ok(new { status = "ok" }));

// Map the reverse proxy - default policy will require authentication
app.MapReverseProxy();

app.Run();