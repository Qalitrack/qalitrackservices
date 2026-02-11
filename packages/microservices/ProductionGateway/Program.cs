using Microsoft.AspNetCore.Authorization;
using Yarp.ReverseProxy.Model;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel from appsettings.json
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Configure(builder.Configuration.GetSection("Kestrel"));
});

// Disable HTTPS redirection
builder.Services.Configure<Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionOptions>(options =>
{
    options.HttpsPort = null;
    options.RedirectStatusCode = (int)HttpStatusCode.TemporaryRedirect;
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
        {
            policy.AllowCredentials();
        }
        else
        {
            policy.DisallowCredentials();
        }
    });
});

// Configure logging
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();

// Enable CORS
app.UseCors();

// Add authorization middleware
app.UseAuthorization();

// ONLY expose /health endpoint - DO NOT map "/"
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithMetadata(new AllowAnonymousAttribute());

// Map the reverse proxy with logging
app.MapReverseProxy(proxyPipeline =>
{
    // Add logging inside the proxy pipeline
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