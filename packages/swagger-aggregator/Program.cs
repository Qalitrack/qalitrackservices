using Microsoft.AspNetCore.Cors.Infrastructure;
using Newtonsoft.Json;
using System.Text;
using YamlDotNet.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure to listen on port 80 - override .NET 8 defaults
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80);
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddHttpClient();

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
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseCors("AllowAll");

// Enable serving static files for the aggregated docs UI
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => "Healthy");

// Main aggregated swagger endpoint
app.MapGet("/swagger.json", async (HttpContext context, IHttpClientFactory httpClientFactory) =>
{
    var clientCode = Environment.GetEnvironmentVariable("CLIENT_CODE") ?? "testing";
    var configPath = Path.Combine("configs", "clients", $"{clientCode}.yml");
    
    var aggregatedSwagger = new
    {
        openapi = "3.0.1",
        info = new
        {
            title = "QaliTrack Aggregated API Documentation",
            description = "Complete API documentation for all QaliTrack microservices",
            version = "1.0.0",
            contact = new
            {
                name = "QaliTrack Support",
                email = "support@qalitrack.com"
            }
        },
        servers = new[]
        {
            new
            {
                url = "http://localhost:7000",
                description = "QaliTrack API Gateway"
            }
        },
        paths = new Dictionary<string, object>(),
        components = new
        {
            schemas = new Dictionary<string, object>(),
            securitySchemes = new
            {
                Bearer = new
                {
                    type = "http",
                    scheme = "bearer",
                    bearerFormat = "JWT",
                    description = "JWT Authorization header using the Bearer scheme"
                }
            }
        },
        tags = new List<object>()
    };

    var enabledServices = await GetEnabledServicesAsync(configPath);
    var httpClient = httpClientFactory.CreateClient();
    httpClient.Timeout = TimeSpan.FromSeconds(10);

    // Add gateway swagger first
    await MergeSwaggerAsync(httpClient, "http://gateway/swagger/v1/swagger.json", "Gateway", aggregatedSwagger);
    
    // Add enabled service swagger docs
    foreach (var service in enabledServices)
    {
        var swaggerUrl = $"http://{service}/swagger/v1/swagger.json";
        await MergeSwaggerAsync(httpClient, swaggerUrl, service, aggregatedSwagger);
    }

    context.Response.ContentType = "application/json";
    return JsonConvert.SerializeObject(aggregatedSwagger, Formatting.Indented);
});

// Swagger UI endpoint
app.MapGet("/", async context =>
{
    var html = GenerateSwaggerUI();
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(html);
});

// Individual service swagger endpoints
app.MapGet("/services/{serviceName}/swagger.json", async (string serviceName, HttpContext context, IHttpClientFactory httpClientFactory) =>
{
    var httpClient = httpClientFactory.CreateClient();
    httpClient.Timeout = TimeSpan.FromSeconds(10);
    
    try
    {
        string swaggerUrl;
        if (serviceName.Equals("gateway", StringComparison.OrdinalIgnoreCase))
        {
            swaggerUrl = "http://gateway/swagger/v1/swagger.json";
        }
        else
        {
            swaggerUrl = $"http://{serviceName}/swagger/v1/swagger.json";
        }
        
        var response = await httpClient.GetAsync(swaggerUrl);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            context.Response.ContentType = "application/json";
            return content;
        }
        else
        {
            context.Response.StatusCode = 404;
            return $"Swagger documentation not found for service: {serviceName}";
        }
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        return $"Error fetching swagger for {serviceName}: {ex.Message}";
    }
});

// Service list endpoint
app.MapGet("/services", async (IHttpClientFactory httpClientFactory) =>
{
    var clientCode = Environment.GetEnvironmentVariable("CLIENT_CODE") ?? "testing";
    var configPath = Path.Combine("configs", "clients", $"{clientCode}.yml");
    var enabledServices = await GetEnabledServicesAsync(configPath);
    
    var services = new List<object> { new { name = "gateway", title = "API Gateway", available = true } };
    
    var httpClient = httpClientFactory.CreateClient();
    httpClient.Timeout = TimeSpan.FromSeconds(5);
    
    foreach (var service in enabledServices)
    {
        var isHealthy = await CheckServiceHealthAsync(httpClient, service);
        services.Add(new { 
            name = service, 
            title = FormatServiceTitle(service),
            available = isHealthy 
        });
    }
    
    return services;
});

async Task<List<string>> GetEnabledServicesAsync(string configPath)
{
    var services = new List<string>();
    
    if (File.Exists(configPath))
    {
        try
        {
            var deserializer = new Deserializer();
            var configContent = await File.ReadAllTextAsync(configPath);
            var config = deserializer.Deserialize<dynamic>(configContent);
            
            if (config.services != null)
            {
                foreach (var service in config.services)
                {
                    var serviceName = service.Key;
                    var serviceConfig = service.Value;
                    
                    if (serviceConfig.enabled == true && serviceName != "gateway")
                    {
                        services.Add(serviceName);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Fallback to known services
            Console.WriteLine($"Error reading config: {ex.Message}");
            services.AddRange(new[] { "user-service", "customer-service" });
        }
    }
    else
    {
        // Fallback services
        services.AddRange(new[] { "user-service", "customer-service" });
    }
    
    return services;
}

async Task<bool> CheckServiceHealthAsync(HttpClient httpClient, string serviceName)
{
    try
    {
        var healthUrl = $"http://{serviceName}/health";
        var response = await httpClient.GetAsync(healthUrl);
        return response.IsSuccessStatusCode;
    }
    catch
    {
        return false;
    }
}

string FormatServiceTitle(string serviceName)
{
    return string.Join(" ", serviceName.Split('-')
        .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
}

async Task MergeSwaggerAsync(HttpClient httpClient, string swaggerUrl, string serviceName, dynamic aggregatedSwagger)
{
    try
    {
        var response = await httpClient.GetAsync(swaggerUrl);
        if (!response.IsSuccessStatusCode) return;
        
        var content = await response.Content.ReadAsStringAsync();
        var serviceSwagger = JsonConvert.DeserializeObject<dynamic>(content);
        
        if (serviceSwagger?.paths != null)
        {
            // Add enhanced service tag with endpoint count info
            var pathCount = serviceSwagger.paths?.Count ?? 0;
            var serviceTag = new { 
                name = serviceName, 
                description = $"{FormatServiceTitle(serviceName)} API - {pathCount} endpoints from {FormatServiceTitle(serviceName)}"
            };
            ((List<object>)aggregatedSwagger.tags).Add(serviceTag);
            
            foreach (var path in serviceSwagger.paths)
            {
                // Simply add all paths without modifying tags to avoid serialization issues
                aggregatedSwagger.paths[path.Name] = path.Value;
            }
        }
        
        // Merge schemas with service prefix to avoid conflicts
        if (serviceSwagger?.components?.schemas != null)
        {
            foreach (var schema in serviceSwagger.components.schemas)
            {
                var prefixedSchema = $"{serviceName.Replace("-", "")}{schema.Name}";
                ((Dictionary<string, object>)aggregatedSwagger.components.schemas)[prefixedSchema] = schema.Value;
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error merging swagger for {serviceName}: {ex.Message}");
    }
}

string GenerateSwaggerUI()
{
    return @"
<!DOCTYPE html>
<html>
<head>
    <title>QaliTrack API Documentation</title>
    <link rel=""stylesheet"" type=""text/css"" href=""https://unpkg.com/swagger-ui-dist@3.52.5/swagger-ui.css"" />
    <style>
        html { box-sizing: border-box; overflow: -moz-scrollbars-vertical; overflow-y: scroll; }
        *, *:before, *:after { box-sizing: inherit; }
        body { margin:0; background: #fafafa; }
        .swagger-ui .topbar { background-color: #1e3a8a; }
        .swagger-ui .topbar .download-url-wrapper { display: none; }
    </style>
</head>
<body>
    <div id=""swagger-ui""></div>
    <script src=""https://unpkg.com/swagger-ui-dist@3.52.5/swagger-ui-bundle.js""></script>
    <script src=""https://unpkg.com/swagger-ui-dist@3.52.5/swagger-ui-standalone-preset.js""></script>
    <script>
        window.onload = function() {
            const currentPath = window.location.pathname.endsWith('/') 
                ? window.location.pathname 
                : window.location.pathname + '/';
            
            const ui = SwaggerUIBundle({
                url: currentPath + 'swagger.json',
                dom_id: '#swagger-ui',
                deepLinking: true,
                presets: [
                    SwaggerUIBundle.presets.apis,
                    SwaggerUIStandalonePreset
                ],
                plugins: [
                    SwaggerUIBundle.plugins.DownloadUrl
                ],
                layout: ""StandaloneLayout"",
                docExpansion: ""list"",
                defaultModelsExpandDepth: 2,
                defaultModelExpandDepth: 2,
                tryItOutEnabled: true,
                supportedSubmitMethods: ['get', 'post', 'put', 'delete', 'patch'],
                onComplete: function() {
                    console.log('QaliTrack API Documentation loaded');
                }
            });
        };
    </script>
</body>
</html>";
}

app.Run();