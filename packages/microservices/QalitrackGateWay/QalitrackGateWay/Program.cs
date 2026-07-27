using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Yarp.ReverseProxy.Model;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;

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

// Get JWT configuration from environment
var jwtSecretKey = builder.Configuration["JWT_SECRET_KEY"] 
    ?? throw new InvalidOperationException("JWT_SECRET_KEY is not configured");
var jwtIssuer = builder.Configuration["JWT_ISSUER"] ?? "qalitrack";
var jwtAudience = builder.Configuration["JWT_AUDIENCE"] ?? "qalitrack";

var key = Encoding.ASCII.GetBytes(jwtSecretKey);

// Add Authentication/Authorization
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// All routes are open - no JWT required at the gateway
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAssertion(_ => true)
        .Build();

    options.AddPolicy("Public", policy => policy.RequireAssertion(_ => true));
});

// Add YARP Reverse Proxy from configuration
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Outbound client for forwarding audit log entries to user-service — the
// gateway itself has no database, so it's just a fire-and-forget HTTP call.
builder.Services.AddHttpClient("AuditLog", client =>
{
    var auditLogBaseUrl = builder.Configuration["AuditLog:UserServiceBaseUrl"] ?? "http://user-service-prod:80";
    client.BaseAddress = new Uri(auditLogBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
});

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
        policy.AllowAnyOrigin()
              .WithMethods(allowedMethods)
              .WithHeaders(allowedHeaders);
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

app.UseAuthentication();
app.UseAuthorization();

// Audit log: record every mutating request (GET is skipped — read traffic
// isn't audited) after auth so HttpContext.User claims are populated. The
// gateway has no database, so this forwards a fire-and-forget HTTP call to
// user-service; a slow or failed audit write never delays or breaks the
// actual proxied request.
app.Use(async (context, next) =>
{
    if (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method))
    {
        await next();
        return;
    }

    var method = context.Request.Method;
    var path = context.Request.Path.Value ?? string.Empty;
    var queryString = context.Request.QueryString.Value ?? string.Empty;
    var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();

    await next();

    stopwatch.Stop();

    var statusCode = context.Response.StatusCode;
    var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var userName = context.User?.FindFirst(ClaimTypes.Email)?.Value;

    var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

    _ = Task.Run(async () =>
    {
        try
        {
            var client = httpClientFactory.CreateClient("AuditLog");
            var payload = new
            {
                method,
                path,
                queryString,
                statusCode,
                userId,
                userName,
                ipAddress,
                durationMs = stopwatch.ElapsedMilliseconds
            };
            await client.PostAsJsonAsync("/AuditLogs", payload);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[Audit] Failed to record audit log for {Method} {Path}", method, path);
        }
    });
});

// Expose a simple health endpoint (anonymous)
app.MapGet("/", () => Results.Ok(new { status = "ok", service = "QaliTrack Gateway" }))
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