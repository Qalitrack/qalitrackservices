using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Qalitrack.Models;
using Qalitrack.Services;

var builder = WebApplication.CreateBuilder(args);

// Enable running as a Windows Service or Linux systemd daemon
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    builder.Host.UseWindowsService(options =>
    {
        options.ServiceName = "QalitrackPlatformService";
    });
}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    builder.Host.UseSystemd();
}

// Configure logging for service mode
builder.Logging.ClearProviders();

// Set minimum log levels
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Console logging (always enabled for debugging)
builder.Logging.AddConsole();

// Platform-specific logging
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    // Windows Event Log with custom source name
#pragma warning disable CA1416
    builder.Logging.AddEventLog(settings =>
    {
        settings.SourceName = "QalitrackPlatformService";
        settings.LogName = "Application";
        settings.Filter = (source, level) => level >= LogLevel.Information;
    });
#pragma warning restore CA1416
}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    // Linux systemd journal
    builder.Logging.AddSystemdConsole(options =>
    {
        options.IncludeScopes = true;
        options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
    });
}

// Configure specific log levels
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.Hosting", LogLevel.Information);
builder.Logging.AddFilter("System", LogLevel.Warning);
builder.Logging.AddFilter("Qalitrack", LogLevel.Information);

// Configure and bind settings
// Priority: External Config File > appsettings.json (defaults)
var runtimeConfig = new RuntimeConfiguration();

// Load defaults from appsettings.json (compiled with app)
if (builder.Configuration.GetSection("TcpListener").Exists())
{
    builder.Configuration.Bind("TcpListener", runtimeConfig.TcpListener);
}
if (builder.Configuration.GetSection("CameraSettings").Exists())
{
    builder.Configuration.Bind("CameraSettings", runtimeConfig.CameraSettings);
}

// Create temporary logger for config loading
var tempLoggerFactory = LoggerFactory.Create(b => b.AddConsole());
var tempLogger = tempLoggerFactory.CreateLogger<RuntimeConfigurationManager>();

// Create and register ConfigurationManager with auto-reload support
var configManager = new RuntimeConfigurationManager(tempLogger);

// Load from external config file (NOT compiled - can change without rebuild)
runtimeConfig = configManager.LoadConfiguration(runtimeConfig);

// Override with environment variables (highest priority - for Docker/cloud)
// Format: QALITRACK_TCP_IP, QALITRACK_TCP_PORT, QALITRACK_CAMERA_URL, etc.
var tcpIp = Environment.GetEnvironmentVariable("QALITRACK_TCP_IP");
if (!string.IsNullOrWhiteSpace(tcpIp))
{
    runtimeConfig.TcpListener.IpAddress = tcpIp;
    tempLogger.LogInformation("TCP IP overridden from environment: {ip}", tcpIp);
}

var tcpPort = Environment.GetEnvironmentVariable("QALITRACK_TCP_PORT");
if (!string.IsNullOrWhiteSpace(tcpPort) && int.TryParse(tcpPort, out int port))
{
    runtimeConfig.TcpListener.Port = port;
    tempLogger.LogInformation("TCP Port overridden from environment: {port}", port);
}

// Bind serial settings from TcpListener config if present
if (builder.Configuration.GetSection("TcpListener:SerialPort").Exists())
{
    var serialPort = builder.Configuration["TcpListener:SerialPort"];
    if (!string.IsNullOrWhiteSpace(serialPort))
    {
        runtimeConfig.TcpListener.SerialPort = serialPort;
        tempLogger.LogInformation("Serial port configured: {port}", serialPort);
    }
}

if (builder.Configuration.GetSection("TcpListener:BaudRate").Exists())
{
    if (int.TryParse(builder.Configuration["TcpListener:BaudRate"], out int baudRate))
    {
        runtimeConfig.TcpListener.BaudRate = baudRate;
    }
}

// Use the runtime configuration
var connectionSettings = runtimeConfig.TcpListener;
var cameraSettings = runtimeConfig.CameraSettings;

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddSingleton(connectionSettings);
builder.Services.AddSingleton(cameraSettings);

// Register ConfigurationManager for runtime config changes
builder.Services.AddSingleton(configManager);
builder.Services.AddSingleton(runtimeConfig);

// Register DataStreamService as a singleton (for weight data)
builder.Services.AddSingleton<DataStreamService>();

// Register CameraDataStreamService as a singleton (for camera data)
builder.Services.AddSingleton<CameraDataStreamService>();

// Register PlateDataStreamService as a singleton (for plate recognition data)
builder.Services.AddSingleton<PlateDataStreamService>();

// Register StreamingMetrics
builder.Services.AddSingleton<StreamingMetrics>();

// Register for proper disposal
builder.Services.AddSingleton<IHostedService>(serviceProvider => 
    new BackgroundServiceWrapper<PlateDataStreamService>(
        serviceProvider.GetRequiredService<PlateDataStreamService>(),
        serviceProvider.GetService<ILogger<BackgroundServiceWrapper<PlateDataStreamService>>>()
    )
);

// Add HTTP client services
builder.Services.AddHttpClient();

// Register PlatformDataService as a singleton and hosted service
builder.Services.AddSingleton<PlatformDataService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<PlatformDataService>());

// Register CameraStreamService as a singleton and hosted service
builder.Services.AddSingleton<CameraStreamService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<CameraStreamService>());

// NPR cameras use HTTP push webhooks - no polling services needed
// Webhook endpoint: /devicemanagement/php/plateresult.php

// Add response compression for SSE
builder.Services.AddResponseCompression(options =>
{
    options.MimeTypes = new[] { "text/event-stream" };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Configure Kestrel to listen on all network interfaces
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5000);
});

var app = builder.Build();

// Setup configuration reload handler AFTER app is built
var reloadLogger = app.Services.GetRequiredService<ILogger<Program>>();
configManager.ConfigurationChanged += (newConfig) =>
{
    try
    {
        reloadLogger.LogInformation("═══════════════════════════════════════════════════════════════");
        reloadLogger.LogInformation("Configuration file changed, reloading settings...");
        reloadLogger.LogInformation("═══════════════════════════════════════════════════════════════");
        
        // Update connection settings
        var platformService = app.Services.GetService<PlatformDataService>();
        if (platformService != null && newConfig.TcpListener != null)
        {
            var newTcp = newConfig.TcpListener;
            if (!string.IsNullOrWhiteSpace(newTcp.IpAddress))
            {
                reloadLogger.LogInformation("→ Updating TCP settings: {ip}:{port}", 
                    newTcp.IpAddress, newTcp.Port);
                platformService.ForceTcpSettings(newTcp.IpAddress, newTcp.Port);
            }
            
            // Update serial settings if changed
            if (!string.IsNullOrWhiteSpace(newTcp.SerialPort))
            {
                reloadLogger.LogInformation("→ Serial port updated: {port} @ {baud} baud", 
                    newTcp.SerialPort, newTcp.BaudRate);
            }
        }

        // Camera settings would be updated similarly
        var cameraService = app.Services.GetService<CameraStreamService>();
        if (cameraService != null && newConfig.CameraSettings != null)
        {
            reloadLogger.LogInformation("→ Camera settings updated (restart service for full effect)");
            // Note: Full camera reload may require service restart
        }
        
        reloadLogger.LogInformation("✓ Configuration reload complete");
        reloadLogger.LogInformation("═══════════════════════════════════════════════════════════════");
    }
    catch (Exception ex)
    {
        reloadLogger.LogError(ex, "✗ Error applying configuration changes");
    }
};

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseCors("AllowAll");

var logger = app.Services.GetRequiredService<ILogger<Program>>();
var urls = builder.Configuration["ASPNETCORE_URLS"]?.Split(';') ?? new[] { "http://localhost:5000" };

// Log the URLs the application is listening on
foreach (var url in urls)
{
    if (!string.IsNullOrWhiteSpace(url))
    {
        var cleanUrl = url.Trim();
        logger.LogInformation("API available at: {url}", cleanUrl);
        
        // Also print to console for better visibility
        Console.WriteLine($"\n🚀 Application is running at: {cleanUrl}");
        Console.WriteLine($"📡 SSE Endpoint: {cleanUrl.TrimEnd('/')}/api/PlatformData/stream\n");
    }
}

logger.LogInformation("Qalitrack Platform Service starting on {os}", RuntimeInformation.OSDescription);

// Check and log administrator/root status
var isAdmin = PrivilegeChecker.IsRunningAsAdministrator();

if (isAdmin)
{
    var role = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Administrator (LocalSystem)" : "root";
    logger.LogInformation("✓ Running with elevated privileges as {role}", role);
}
else
{
    logger.LogWarning("✗ NOT running with administrator/root privileges!");
    logger.LogWarning("Serial ports and system features may be restricted!");
}

// Log configuration file details
logger.LogInformation("═══════════════════════════════════════════════════════════════");
logger.LogInformation("Configuration Management:");
logger.LogInformation("  File: {path}", configManager.GetConfigFilePath());
logger.LogInformation("  Exists: {exists}", configManager.ConfigFileExists() ? "Yes ✓" : "No (using defaults)");
if (configManager.ConfigFileExists())
{
    var lastModified = configManager.GetLastModifiedTime();
    if (lastModified.HasValue)
    {
        logger.LogInformation("  Last modified: {time}", lastModified.Value.ToLocalTime());
    }
}
logger.LogInformation("  Auto-reload: Enabled ✓");
logger.LogInformation("  You can edit this file while service is running!");
logger.LogInformation("═══════════════════════════════════════════════════════════════");

app.UseResponseCompression();
app.UseStaticFiles();
app.UseRouting();

// CORS must be after UseRouting() and before UseEndpoints()
app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();

// Helper: Get all local IPs
IEnumerable<string> GetLocalIps() =>
    NetworkInterface.GetAllNetworkInterfaces()
        .SelectMany(x => x.GetIPProperties().UnicastAddresses)
        .Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork &&
                   !IPAddress.IsLoopback(x.Address))
        .Select(x => x.Address.ToString())
        .Distinct();

// Root endpoint — shows all available URLs
app.MapGet("/", async (HttpContext context) =>
{
    var scheme = context.Request.Scheme;
    var port = context.Request.Host.Port ?? (scheme == "https" ? 443 : 80);
    var localIps = GetLocalIps().ToList();

    var response = new System.Text.StringBuilder();
    response.AppendLine("╔════════════════════════════════════════════════════════════════╗");
    response.AppendLine("║     Qalitrack Platform Data Service - RUNNING ✓               ║");
    response.AppendLine("╚════════════════════════════════════════════════════════════════╝");
    response.AppendLine();
    response.AppendLine($"Platform: {RuntimeInformation.OSDescription}");
    response.AppendLine($"Runtime:  {RuntimeInformation.FrameworkDescription}");
    response.AppendLine($"Config:   {configManager.GetConfigFilePath()}");
    response.AppendLine($"Auto-reload: Enabled ✓ (edit config anytime!)");
    response.AppendLine();
    response.AppendLine("═══════════════════════════════════════════════════════════════");
    response.AppendLine("                    ACTIVE SERVICES");
    response.AppendLine("═══════════════════════════════════════════════════════════════");
    response.AppendLine();
    response.AppendLine("✓ PlatformDataService     → Weight data (TCP/Serial)");
    response.AppendLine("✓ CameraStreamService     → Video streams & snapshots");
    response.AppendLine("✓ DataStreamService       → SSE broadcasting (weight)");
    response.AppendLine("✓ CameraDataStreamService → SSE broadcasting (camera frames)");
    response.AppendLine("✓ PlateDataStreamService  → SSE broadcasting (NPR plates)");
    response.AppendLine();
    response.AppendLine("═══════════════════════════════════════════════════════════════");
    response.AppendLine("                    CONFIGURED CAMERAS");
    response.AppendLine("═══════════════════════════════════════════════════════════════");
    response.AppendLine();

    foreach (var camera in cameraSettings.Cameras.Where(c => c.Enabled))
    {
        response.AppendLine($"📷 {camera.Name} ({camera.Id})");
        response.AppendLine($"   IP: {camera.IpAddress}:{camera.Port}");
        response.AppendLine($"   Stream: {camera.RtspPath}");
        if (camera.NprSettings?.Enabled == true)
        {
            response.AppendLine($"   NPR: Enabled ✓");
        }
        response.AppendLine();
    }

    response.AppendLine("═══════════════════════════════════════════════════════════════");
    response.AppendLine("                    AVAILABLE ENDPOINTS");
    response.AppendLine("═══════════════════════════════════════════════════════════════");
    response.AppendLine();

    var firstIp = localIps.FirstOrDefault() ?? "localhost";
    var baseUrl = $"{scheme}://{firstIp}:{port}";

    response.AppendLine("📊 WEIGHT DATA (SSE Stream):");
    response.AppendLine($"   {baseUrl}/api/PlatformData/stream");
    response.AppendLine($"   {baseUrl}/sse-test.html");
    response.AppendLine();

    response.AppendLine("📡 PLATE DATA (SSE Stream - Real-time NPR):");
    response.AppendLine($"   {baseUrl}/api/PlatformData/plates/stream");
    response.AppendLine();

    response.AppendLine("📹 CAMERAS:");
    response.AppendLine($"   {baseUrl}/api/Camera/cameras              → List all cameras");
    response.AppendLine($"   {baseUrl}/api/Camera/{{id}}/snapshot        → Get snapshot");
    response.AppendLine($"   {baseUrl}/api/Camera/{{id}}/stream          → Live MJPEG stream");
    response.AppendLine($"   {baseUrl}/api/Camera/{{id}}/status          → Camera status");
    response.AppendLine();

    response.AppendLine("🔧 CONFIGURATION:");
    response.AppendLine($"   {baseUrl}/config.html                    → Config UI");
    response.AppendLine($"   {baseUrl}/api/configuration              → Config API");
    response.AppendLine();

    response.AppendLine("═══════════════════════════════════════════════════════════════");
    response.AppendLine("                         EXAMPLES");
    response.AppendLine("═══════════════════════════════════════════════════════════════");
    response.AppendLine();
    response.AppendLine("📋 CURL - Listen for Weight Data:");
    response.AppendLine($"   curl -N {baseUrl}/api/PlatformData/stream");
    response.AppendLine();
    response.AppendLine("📋 CURL - Listen for Plate Data:");
    response.AppendLine($"   curl -N {baseUrl}/api/PlatformData/plates/stream");
    response.AppendLine();
    response.AppendLine("📋 JavaScript - Weight Data:");
    response.AppendLine($"   const es = new EventSource('{baseUrl}/api/PlatformData/stream');");
    response.AppendLine("   es.onmessage = (e) => console.log('Weight:', e.data);");
    response.AppendLine();
    response.AppendLine("📋 JavaScript - Plate Data:");
    response.AppendLine($"   const es = new EventSource('{baseUrl}/api/PlatformData/plates/stream');");
    response.AppendLine("   es.onmessage = (e) => console.log('Plate:', JSON.parse(e.data));");
    response.AppendLine();

    if (localIps.Count() > 1)
    {
        response.AppendLine("═══════════════════════════════════════════════════════════════");
        response.AppendLine("              ADDITIONAL NETWORK INTERFACES");
        response.AppendLine("═══════════════════════════════════════════════════════════════");
        response.AppendLine();
        foreach (var ip in localIps.Skip(1))
        {
            response.AppendLine($"🌐 {ip}:{port}");
        }
        response.AppendLine();
    }

    context.Response.ContentType = "text/plain; charset=utf-8";
    await context.Response.WriteAsync(response.ToString());
});

// Auto-configure firewall on Linux only
if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    _ = Task.Run(async () =>
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"sudo ufw allow 5000/tcp 2>/dev/null || true\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var p = System.Diagnostics.Process.Start(psi);
            if (p != null) await p.WaitForExitAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to configure firewall automatically");
        }
    });
}

logger.LogInformation("Service configured and ready to start");

app.Run();

// Cleanup
tempLoggerFactory.Dispose();