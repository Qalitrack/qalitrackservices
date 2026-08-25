using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Yarp.ReverseProxy.Model;
using System.Linq;
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

// Read the real client IP/scheme from X-Forwarded-* headers instead of the
// TCP connection — but ONLY from a proxy we actually know about. Most
// installs are offline Docker deployments where this gateway container IS
// the edge (nothing in front of it), so trusting X-Forwarded-For by default
// would let any client spoof their own logged IP. Deployments that do sit
// behind a real reverse proxy (e.g. the cloud K8s install behind Traefik)
// opt in by listing that proxy under ForwardedHeaders:KnownProxies /
// KnownNetworks in their own config — with none configured, the default
// ASP.NET Core behavior (trust nothing beyond loopback) applies, and
// Connection.RemoteIpAddress is left as the real client IP untouched.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = builder.Configuration.GetValue<ForwardedHeaders?>("ForwardedHeaders:ForwardedHeaders") ?? ForwardedHeaders.All;
    options.ForwardLimit = 1;

    // List<IPAddress>/List<IPNetwork> aren't config-binder-convertible types,
    // so KnownProxies/KnownNetworks are parsed explicitly here rather than
    // via Configuration.Bind().
    foreach (var proxy in builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? Array.Empty<string>())
    {
        if (IPAddress.TryParse(proxy, out var ip)) options.KnownProxies.Add(ip);
    }

    foreach (var network in builder.Configuration.GetSection("ForwardedHeaders:KnownNetworks").Get<string[]>() ?? Array.Empty<string>())
    {
        var parts = network.Split('/');
        if (parts.Length == 2 && IPAddress.TryParse(parts[0], out var prefix) && int.TryParse(parts[1], out var prefixLength))
        {
            options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(prefix, prefixLength));
        }
    }
});

var app = builder.Build();

// Must run before anything reads Connection.RemoteIpAddress or Request.Scheme —
// in particular, before the audit-log middleware below.
app.UseForwardedHeaders();

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

// Classifies an incoming path into (EntityType, EntityId) for audit logging.
// Mirrors the PathRemovePrefix transforms already in this file's
// ReverseProxy:Routes config, so it stays in sync with actual routing.
static (string? EntityType, string? EntityId) ClassifyPath(string path)
{
    var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
    var i = 0;
    if (i < segments.Length && segments[i].Equals("api", StringComparison.OrdinalIgnoreCase)) i++;

    string[][] wrapperPrefixes =
    {
        new[] { "MasterData" },
        new[] { "Transaction" },
        new[] { "tech", "user" }
    };
    foreach (var wrapper in wrapperPrefixes)
    {
        if (i + wrapper.Length <= segments.Length &&
            wrapper.SequenceEqual(segments.Skip(i).Take(wrapper.Length), StringComparer.OrdinalIgnoreCase))
        {
            i += wrapper.Length;
            break;
        }
    }

    if (i >= segments.Length) return (null, null);
    var entityType = segments[i];
    var entityId = (i + 1 < segments.Length && LooksLikeId(segments[i + 1])) ? segments[i + 1] : null;
    return (entityType, entityId);
}

static bool LooksLikeId(string s) => Guid.TryParse(s, out _) || s.All(char.IsDigit);

// Entity types considered sensitive enough that even a read of them should be
// audited (PII / financial data) — everything else keeps read traffic unaudited.
var sensitiveReadEntityTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "Drivers", "Vehicles", "Suppliers", "Customers", "Owners", "Transporters",
    "Users", "Roles", "Permissions", "Transaction", "AuditLogs"
};

// Audit log: record every mutating request, plus reads of sensitive entities
// (HEAD is always skipped — it carries no information beyond a GET), after
// auth so HttpContext.User claims are populated. The gateway has no database,
// so this forwards a fire-and-forget HTTP call to user-service; a slow or
// failed audit write never delays or breaks the actual proxied request.
app.Use(async (context, next) =>
{
    var method = context.Request.Method;
    var path = context.Request.Path.Value ?? string.Empty;
    var (entityType, entityId) = ClassifyPath(path);

    if (HttpMethods.IsHead(method))
    {
        await next();
        return;
    }

    var isGet = HttpMethods.IsGet(method);
    var isSensitiveRead = isGet && entityType != null && sensitiveReadEntityTypes.Contains(entityType);
    if (isGet && !isSensitiveRead)
    {
        await next();
        return;
    }

    var action = method switch
    {
        _ when isGet => "Read",
        _ when HttpMethods.IsPost(method) => "Create",
        _ when HttpMethods.IsPut(method) => "Update",
        _ when HttpMethods.IsPatch(method) => "Update",
        _ when HttpMethods.IsDelete(method) => "Delete",
        _ => method
    };

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
                durationMs = stopwatch.ElapsedMilliseconds,
                entityType,
                entityId,
                action
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