using Microsoft.AspNetCore.Authorization;
using Yarp.ReverseProxy.Model;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on both HTTP and HTTPS
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // HTTP port 80 for redirection
    serverOptions.ListenAnyIP(80);

    // HTTPS port 443
    serverOptions.ListenAnyIP(443, listenOptions =>
    {
        listenOptions.UseHttps(); // Certificate configured in appsettings.json or default
    });
});

// Add HTTPS redirection (redirect all HTTP to HTTPS)
builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect; // Permanent redirect
    options.HttpsPort = 443; // Port where HTTPS is served
});

// Add HSTS (HTTP Strict Transport Security)
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

// Add Authorization policies BEFORE YARP
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Public", policy => policy.RequireAssertion(_ => true));
});

// Add YARP Reverse Proxy from configuration
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Configure CORS
var corsSection = builder.Configuration.GetSection("Cors");
var allowedOrigins = corsSection.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
var allowedMethods = corsSection.GetSection("AllowedMethods").Get<string[]>() ?? new[] { "GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS" };
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
            policy.AllowCredentials();
        else
            policy.DisallowCredentials();
    });
});

// Configure logging
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();

// Enforce HSTS
app.UseHsts();

// Redirect HTTP → HTTPS
app.UseHttpsRedirection();

// Enable CORS
app.UseCors();

// Add authorization middleware
app.UseAuthorization();

// Health endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithMetadata(new AllowAnonymousAttribute());

// Reverse proxy with logging
app.MapReverseProxy(proxyPipeline =>
{
    proxyPipeline.Use(async (context, next) =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogInformation($"[YARP] Proxying {context.Request.Method} {context.Request.Path}{context.Request.QueryString}");
        await next();
    });

    proxyPipeline.UseSessionAffinity();
    proxyPipeline.UseLoadBalancing();
    proxyPipeline.UsePassiveHealthChecks();
});

app.Run();
