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

// Don't hardcode port - let environment variable control it
// REMOVED: builder.WebHost.UseUrls("http://*:7000");

// Disable HTTPS redirection
builder.Services.Configure<Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionOptions>(options =>
{
    options.HttpsPort = null;
    options.RedirectStatusCode = (int)HttpStatusCode.TemporaryRedirect;
});

// Add Authorization policies BEFORE YARP (CRITICAL FIX)
builder.Services.AddAuthorization(options =>
{
    // Add a policy for public endpoints that allows all access
    options.AddPolicy("Public", policy => policy.RequireAssertion(_ => true));
    
    // Optional: Add other policies if needed
    // options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
});

// Add YARP Reverse Proxy from configuration (AFTER Authorization)
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Configure CORS
var corsSection = builder.Configuration.GetSection("Cors");
var allowedOrigins = corsSection.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
var allowedMethods = corsSection.GetSection("AllowedMethods").Get<string[]>() ?? new[] { "GET", "POST", "PUT", "DELETE" };
var allowedHeaders = corsSection.GetSection("AllowedHeaders").Get<string[]>() ?? new[] { "*" };
var allowCredentials = corsSection.GetValue<bool>("AllowCredentials");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .WithMethods(allowedMethods)
              .WithHeaders(allowedHeaders);

        if (allowCredentials)
        {
            policy.AllowCredentials();
        }
        else
        {
            policy.DisallowCredentials();
        }
    });
});

// Configure logging for the application
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();

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

// Enable CORS - moved after logging middleware
app.UseCors();

// Add authorization middleware
app.UseAuthorization();

// Expose a simple health endpoint
app.MapGet("/", () => Results.Ok(new { status = "ok", service = "QaliTrack Production Gateway" }))
    .WithMetadata(new AllowAnonymousAttribute());

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithMetadata(new AllowAnonymousAttribute());

// Map the reverse proxy
app.MapReverseProxy(proxyPipeline =>
{
    proxyPipeline.UseSessionAffinity();
    proxyPipeline.UseLoadBalancing();
    proxyPipeline.UsePassiveHealthChecks();
});

app.Run();