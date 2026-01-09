using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Yarp.ReverseProxy.Model;
using System.Net;
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

// Add Authorization with a default policy that requires authentication
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .Build();
    
    // Add a policy for anonymous endpoints
    options.AddPolicy("Anonymous", policy => policy.RequireAssertion(_ => true));
});

// Add YARP Reverse Proxy from configuration
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

app.UseAuthentication();
app.UseAuthorization();

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