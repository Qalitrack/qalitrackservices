using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using Qalitrack.Models;
using Qalitrack.Services;

// ─── Installer / CLI mode detection ──────────────────────────────────────────
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    var arg = args.FirstOrDefault()?.ToLowerInvariant();

    if (arg == "--install" || (arg == null && Environment.UserInteractive))
    {
#if WINDOWS
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new Qalitrack.Installer.InstallerWizard());
#endif
        return;
    }

    if (arg is "--uninstall" or "--status" or "--start" or "--stop")
    {
        AttachConsole();
        HandleCliCommand(arg);
        return;
    }
}

// ─── Service Mode ─────────────────────────────────────────────────────────────
var builder = WebApplication.CreateBuilder(args);

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

// ─── Logging ──────────────────────────────────────────────────────────────────
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Logging.AddConsole();

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
#pragma warning disable CA1416
    builder.Logging.AddEventLog(settings =>
    {
        settings.SourceName = "QalitrackPlatformService";
        settings.LogName    = "Application";
        settings.Filter     = (_, level) => level >= LogLevel.Information;
    });
#pragma warning restore CA1416
}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    builder.Logging.AddSystemdConsole(options =>
    {
        options.IncludeScopes   = true;
        options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
    });
}

builder.Logging.AddFilter("Microsoft",         LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.Hosting", LogLevel.Information);
builder.Logging.AddFilter("System",            LogLevel.Warning);
builder.Logging.AddFilter("Qalitrack",         LogLevel.Information);

// ─── Configuration ────────────────────────────────────────────────────────────
var tempLoggerFactory = LoggerFactory.Create(b => b.AddConsole());
var tempLogger        = tempLoggerFactory.CreateLogger<RuntimeConfigurationManager>();

var configManager = new RuntimeConfigurationManager(tempLogger);
var runtimeConfig = configManager.LoadConfiguration(new RuntimeConfiguration());

ApplyEnvironmentOverrides(runtimeConfig, tempLogger);

var connectionSettings = runtimeConfig.TcpListener;
var cameraSettings     = runtimeConfig.CameraSettings;
var rfidSettings       = runtimeConfig.RfidSettings;
var nfcSettings        = runtimeConfig.NfcSettings;

// ─── DI Registration ──────────────────────────────────────────────────────────
builder.Services.AddControllers();

builder.Services.AddSingleton(connectionSettings);
builder.Services.AddSingleton(cameraSettings);
builder.Services.AddSingleton(rfidSettings);
builder.Services.AddSingleton(nfcSettings);
builder.Services.AddSingleton(configManager);
builder.Services.AddSingleton(runtimeConfig);

// ── StreamingMetrics MUST be registered first — all stream services depend on it ──
builder.Services.AddSingleton<StreamingMetrics>();
builder.Services.AddSingleton<DataStreamService>();
builder.Services.AddSingleton<CameraDataStreamService>();
builder.Services.AddKeyedSingleton<PlateDataStreamService>("lane1");
builder.Services.AddKeyedSingleton<PlateDataStreamService>("lane2");
builder.Services.AddSingleton<RfidTagStreamService>();
builder.Services.AddSingleton<NfcTagStreamService>();

if (rfidSettings.Enabled)
{
    builder.Services.AddHostedService<RfidReaderBackgroundService>();
    tempLogger.LogInformation("RFID reader enabled — {host}:{port}", rfidSettings.Host, rfidSettings.Port);
}
else
{
    tempLogger.LogInformation("RFID reader disabled in configuration");
}

if (nfcSettings.Enabled)
{
    builder.Services.AddHostedService<NfcReaderBackgroundService>();
    tempLogger.LogInformation("NFC reader enabled — {port} @ {baud} baud", nfcSettings.Port, nfcSettings.BaudRate);
}
else
{
    tempLogger.LogInformation("NFC reader disabled in configuration");
}

builder.Services.AddSingleton<IHostedService>(serviceProvider =>
{
    var service = serviceProvider.GetRequiredKeyedService<PlateDataStreamService>("lane1");
    var logger  = serviceProvider.GetService<ILogger<BackgroundServiceWrapper<PlateDataStreamService>>>();
    return new BackgroundServiceWrapper<PlateDataStreamService>(service, logger);
});

builder.Services.AddSingleton<IHostedService>(serviceProvider =>
{
    var service = serviceProvider.GetRequiredKeyedService<PlateDataStreamService>("lane2");
    var logger  = serviceProvider.GetService<ILogger<BackgroundServiceWrapper<PlateDataStreamService>>>();
    return new BackgroundServiceWrapper<PlateDataStreamService>(service, logger);
});

builder.Services.AddHttpClient();

builder.Services.AddSingleton<PlatformDataService>();
builder.Services.AddHostedService(p => p.GetRequiredService<PlatformDataService>());

builder.Services.AddSingleton<CameraStreamService>();
builder.Services.AddHostedService(p => p.GetRequiredService<CameraStreamService>());

builder.Services.AddResponseCompression(options =>
{
    options.MimeTypes = new[] { "text/event-stream" };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5000);
});

// ─── App Pipeline ─────────────────────────────────────────────────────────────
var app = builder.Build();

var reloadLogger = app.Services.GetRequiredService<ILogger<Program>>();
configManager.ConfigurationChanged += (newConfig) =>
{
    try
    {
        reloadLogger.LogInformation("═══════════════════════════════════════════════════════════════");
        reloadLogger.LogInformation("Configuration file changed — applying updates");
        reloadLogger.LogInformation("═══════════════════════════════════════════════════════════════");

        var platformService = app.Services.GetService<PlatformDataService>();
        if (platformService != null && !string.IsNullOrWhiteSpace(newConfig.TcpListener?.IpAddress))
        {
            reloadLogger.LogInformation("→ TCP: {ip}:{port}",
                newConfig.TcpListener.IpAddress, newConfig.TcpListener.Port);
            platformService.ForceTcpSettings(newConfig.TcpListener.IpAddress, newConfig.TcpListener.Port);
        }

        if (!string.IsNullOrWhiteSpace(newConfig.TcpListener?.SerialPort))
            reloadLogger.LogInformation("→ Serial: {port} @ {baud} baud (restart required)",
                newConfig.TcpListener.SerialPort, newConfig.TcpListener.BaudRate);

        if (newConfig.CameraSettings != null)
            reloadLogger.LogInformation("→ Camera settings updated (restart required)");

        if (newConfig.RfidSettings != null)
            reloadLogger.LogInformation("→ RFID: {host}:{port} (restart required)",
                newConfig.RfidSettings.Host, newConfig.RfidSettings.Port);

        if (newConfig.NfcSettings != null)
            reloadLogger.LogInformation("→ NFC: {port} @ {baud} baud (restart required)",
                newConfig.NfcSettings.Port, newConfig.NfcSettings.BaudRate);

        reloadLogger.LogInformation("✓ Configuration reload complete");
        reloadLogger.LogInformation("═══════════════════════════════════════════════════════════════");
    }
    catch (Exception ex)
    {
        reloadLogger.LogError(ex, "✗ Error applying configuration changes");
    }
};

app.UseCors("AllowAll");
app.UseResponseCompression();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// ─── Startup Logging ──────────────────────────────────────────────────────────
var logger = app.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Qalitrack Platform Service starting on {os}", RuntimeInformation.OSDescription);

var isAdmin = PrivilegeChecker.IsRunningAsAdministrator();
if (isAdmin)
{
    var role = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Administrator (LocalSystem)" : "root";
    logger.LogInformation("✓ Running with elevated privileges as {role}", role);
}
else
{
    logger.LogWarning("✗ NOT running with elevated privileges — serial ports may be restricted");
}

logger.LogInformation("═══════════════════════════════════════════════════════════════");
logger.LogInformation("Configuration:");
logger.LogInformation("  File:          {path}", configManager.GetConfigFilePath());
logger.LogInformation("  Exists:        {exists}", configManager.ConfigFileExists() ? "Yes ✓" : "No (defaults used)");
if (configManager.ConfigFileExists())
{
    var lastModified = configManager.GetLastModifiedTime();
    if (lastModified.HasValue)
        logger.LogInformation("  Last modified: {time}", lastModified.Value.ToLocalTime());
}
logger.LogInformation("  Auto-reload:   Enabled ✓");
logger.LogInformation("═══════════════════════════════════════════════════════════════");
logger.LogInformation("Weighbridge TCP:  {ip}:{port}", connectionSettings.IpAddress, connectionSettings.Port);
logger.LogInformation("Serial Port:      {port} @ {baud} baud", connectionSettings.SerialPort, connectionSettings.BaudRate);
logger.LogInformation("═══════════════════════════════════════════════════════════════");
logger.LogInformation("RFID: {status}", rfidSettings.Enabled ? $"Enabled — {rfidSettings.Host}:{rfidSettings.Port}" : "Disabled");
logger.LogInformation("NFC:  {status}", nfcSettings.Enabled  ? $"Enabled — {nfcSettings.Port} @ {nfcSettings.BaudRate} baud" : "Disabled");
logger.LogInformation("═══════════════════════════════════════════════════════════════");

// ─── Root Info Endpoint ───────────────────────────────────────────────────────
IEnumerable<string> GetLocalIps() =>
    NetworkInterface.GetAllNetworkInterfaces()
        .SelectMany(x => x.GetIPProperties().UnicastAddresses)
        .Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(x.Address))
        .Select(x => x.Address.ToString())
        .Distinct();

app.MapGet("/", async (HttpContext context) =>
{
    var scheme   = context.Request.Scheme;
    var port     = context.Request.Host.Port ?? (scheme == "https" ? 443 : 80);
    var localIps = GetLocalIps().ToList();
    var firstIp  = localIps.FirstOrDefault() ?? "localhost";
    var baseUrl  = $"{scheme}://{firstIp}:{port}";

    var sb = new System.Text.StringBuilder();
    sb.AppendLine("╔════════════════════════════════════════════════════════════════╗");
    sb.AppendLine("║     Qalitrack Platform Data Service - RUNNING ✓               ║");
    sb.AppendLine("╚════════════════════════════════════════════════════════════════╝");
    sb.AppendLine();
    sb.AppendLine($"Platform:    {RuntimeInformation.OSDescription}");
    sb.AppendLine($"Runtime:     {RuntimeInformation.FrameworkDescription}");
    sb.AppendLine($"Config file: {configManager.GetConfigFilePath()}");
    sb.AppendLine($"Auto-reload: Enabled ✓");
    sb.AppendLine();
    sb.AppendLine("═══════════════════════════════════════════════════════════════");
    sb.AppendLine("                       ACTIVE SERVICES");
    sb.AppendLine("═══════════════════════════════════════════════════════════════");
    sb.AppendLine("✓ PlatformDataService         → Weight data (TCP/Serial)");
    sb.AppendLine("✓ CameraStreamService         → Video streams & snapshots");
    sb.AppendLine("✓ DataStreamService           → SSE broadcasting (weight)");
    sb.AppendLine("✓ CameraDataStreamService     → SSE broadcasting (camera)");
    sb.AppendLine("✓ PlateDataStreamService      → SSE broadcasting (NPR)");
    sb.AppendLine(rfidSettings.Enabled
        ? $"✓ RfidReaderBackgroundService → RFID tag reading{Environment.NewLine}  ↳ {rfidSettings.Host}:{rfidSettings.Port}"
        : "✗ RFID Services               → Disabled in config");
    sb.AppendLine(nfcSettings.Enabled
        ? $"✓ NfcReaderBackgroundService  → NFC tag reading{Environment.NewLine}  ↳ {nfcSettings.Port} @ {nfcSettings.BaudRate} baud"
        : "✗ NFC Services                → Disabled in config");
    sb.AppendLine();
    sb.AppendLine("═══════════════════════════════════════════════════════════════");
    sb.AppendLine("                      CONFIGURED CAMERAS");
    sb.AppendLine("═══════════════════════════════════════════════════════════════");
    foreach (var camera in cameraSettings.Cameras.Where(c => c.Enabled))
    {
        sb.AppendLine($"📷 {camera.Name} ({camera.Id})");
        sb.AppendLine($"   IP:     {camera.IpAddress}:{camera.Port}");
        sb.AppendLine($"   Stream: {camera.RtspPath}");
        if (camera.NprSettings?.Enabled == true)
            sb.AppendLine("   NPR:    Enabled ✓");
        sb.AppendLine();
    }
    sb.AppendLine("═══════════════════════════════════════════════════════════════");
    sb.AppendLine("                      AVAILABLE ENDPOINTS");
    sb.AppendLine("═══════════════════════════════════════════════════════════════");
    sb.AppendLine($"📊 Weight SSE:   {baseUrl}/api/PlatformData/stream");
    sb.AppendLine($"📡 Plate SSE (Lane 1):          {baseUrl}/api/plates/stream");
    sb.AppendLine($"📡 Plate SSE (Lane 2):          {baseUrl}/api/plates/lane2/stream");
    sb.AppendLine("─── Camera Webhook URLs (configure in camera settings) ───────");
    sb.AppendLine("  Lane 1:");
    sb.AppendLine("    Plate Result:   /devicemanagement/php/plateresult.php");
    sb.AppendLine("    Quick Plate:    /devicemanagement/php/quickplateresult.php");
    sb.AppendLine("    Device Info:    /devicemanagement/php/receivedeviceinfo.php");
    sb.AppendLine("    GPIO:           /devicemanagement/php/gio.php");
    sb.AppendLine("    Port Trigger:   /devicemanagement/php/porttrigger.php");
    sb.AppendLine("    Gate:           /devicemanagement/php/gate.php");
    sb.AppendLine("    Serial:         /devicemanagement/php/serial.php");
    sb.AppendLine("  Lane 2:");
    sb.AppendLine("    Plate Result:   /devicemanagement/php/lane2/plateresult.php");
    sb.AppendLine("    Quick Plate:    /devicemanagement/php/lane2/quickplateresult.php");
    sb.AppendLine("    Device Info:    /devicemanagement/php/lane2/receivedeviceinfo.php");
    sb.AppendLine("    GPIO:           /devicemanagement/php/lane2/gio.php");
    sb.AppendLine("    Port Trigger:   /devicemanagement/php/lane2/porttrigger.php");
    sb.AppendLine("    Gate:           /devicemanagement/php/lane2/gate.php");
    sb.AppendLine("    Serial:         /devicemanagement/php/lane2/serial.php");
    sb.AppendLine($"📹 Cameras List:     {baseUrl}/api/Camera/cameras");
    sb.AppendLine($"📸 Snapshot:         {baseUrl}/api/Camera/YOUR-ID/snapshot");
    sb.AppendLine($"📡 Status:           {baseUrl}/api/Camera/YOUR-ID/status");
    sb.AppendLine($"🎥 Live Stream:      {baseUrl}/api/Camera/YOUR-ID/stream");
    if (rfidSettings.Enabled)
    {
        sb.AppendLine($"🏷️  RFID SSE:    {baseUrl}/api/rfid/stream");
        sb.AppendLine($"   RFID Health: {baseUrl}/api/rfid/health");
    }
    if (nfcSettings.Enabled)
    {
        sb.AppendLine($"💳 NFC SSE:     {baseUrl}/api/nfc/stream");
        sb.AppendLine($"   NFC Health:  {baseUrl}/api/nfc/health");
    }
    sb.AppendLine($"📹 Cameras:      {baseUrl}/api/Camera/cameras");
    sb.AppendLine($"🔧 Config UI:    {baseUrl}/config.html");
    sb.AppendLine($"🔧 Config API:   {baseUrl}/api/configuration");
    sb.AppendLine();
    sb.AppendLine("═══════════════════════════════════════════════════════════════");
    sb.AppendLine("                          EXAMPLES");
    sb.AppendLine("═══════════════════════════════════════════════════════════════");
    sb.AppendLine($"curl -N {baseUrl}/api/PlatformData/stream");
    if (rfidSettings.Enabled) sb.AppendLine($"curl -N {baseUrl}/api/rfid/stream");
    if (nfcSettings.Enabled)  sb.AppendLine($"curl -N {baseUrl}/api/nfc/stream");
    sb.AppendLine($"const es = new EventSource('{baseUrl}/api/PlatformData/stream');");
    sb.AppendLine("es.onmessage = (e) => console.log('Weight:', e.data);");
    if (localIps.Count > 1)
    {
        sb.AppendLine();
        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        sb.AppendLine("               ADDITIONAL NETWORK INTERFACES");
        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        foreach (var ip in localIps.Skip(1))
            sb.AppendLine($"🌐 {ip}:{port}");
    }

    context.Response.ContentType = "text/plain; charset=utf-8";
    await context.Response.WriteAsync(sb.ToString());
});

// ─── Linux Firewall ───────────────────────────────────────────────────────────
if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    _ = Task.Run(async () =>
    {
        try
        {
            using var p = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName        = "/bin/bash",
                Arguments       = "-c \"sudo ufw allow 5000/tcp 2>/dev/null || true\"",
                UseShellExecute = false,
                CreateNoWindow  = true
            });
            if (p != null) await p.WaitForExitAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to configure firewall automatically — ensure port 5000 is open");
        }
    });
}

logger.LogInformation("Service configured and ready to start");

app.Run();

tempLoggerFactory.Dispose();

// ─── CLI command handler ──────────────────────────────────────────────────────
static void HandleCliCommand(string command)
{
    const string ServiceName = "QalitrackPlatformService";
    Console.WriteLine();
    try
    {
        switch (command)
        {
            case "--status":
                try
                {
                    using var sc = new ServiceController(ServiceName);
                    Console.WriteLine($"  Service : {ServiceName}");
                    Console.WriteLine($"  Status  : {sc.Status}");
                    Console.WriteLine($"  Startup : {sc.StartType}");
                }
                catch { Console.WriteLine($"  Service '{ServiceName}' is not installed."); }
                break;

            case "--start":
                using (var sc = new ServiceController(ServiceName))
                {
                    if (sc.Status == ServiceControllerStatus.Running)
                    { Console.WriteLine("  Already running."); break; }
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(15));
                    Console.WriteLine($"  ✓ Started. Status: {sc.Status}");
                }
                break;

            case "--stop":
                using (var sc = new ServiceController(ServiceName))
                {
                    if (sc.Status == ServiceControllerStatus.Stopped)
                    { Console.WriteLine("  Already stopped."); break; }
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(15));
                    Console.WriteLine($"  ✓ Stopped. Status: {sc.Status}");
                }
                break;

            case "--uninstall":
                Console.Write($"  Uninstall '{ServiceName}'? [y/N] : ");
                var confirm = Console.ReadLine()?.Trim().ToLower();
                if (confirm != "y" && confirm != "yes")
                { Console.WriteLine("  Cancelled."); break; }

                try
                {
                    using var sc = new ServiceController(ServiceName);
                    if (sc.Status != ServiceControllerStatus.Stopped)
                    {
                        Console.WriteLine("  Stopping service...");
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                    }
                }
                catch { /* service may already be gone */ }

                using (var p = System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo("sc.exe", $"delete \"{ServiceName}\"")
                    {
                        UseShellExecute        = false,
                        CreateNoWindow         = true,
                        RedirectStandardOutput = true
                    })!)
                {
                    p.WaitForExit(5000);
                    Console.WriteLine(p.ExitCode == 0
                        ? "  ✓ Service uninstalled."
                        : "  ✗ Could not remove — run as Administrator.");
                }
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ✗ Error: {ex.Message}");
        Console.WriteLine("  Make sure you are running as Administrator.");
    }
    Console.WriteLine();
}

// ─── Attach to parent console so CLI output is visible ────────────────────────
static void AttachConsole()
{
    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    static extern bool AttachConsole(int dwProcessId);
    AttachConsole(-1);
}

// ─── Environment variable overrides ──────────────────────────────────────────
static void ApplyEnvironmentOverrides(RuntimeConfiguration config, ILogger logger)
{
    var tcpIp = Environment.GetEnvironmentVariable("QALITRACK_TCP_IP");
    if (!string.IsNullOrWhiteSpace(tcpIp))
    {
        config.TcpListener.IpAddress = tcpIp;
        logger.LogInformation("ENV override — TCP IP: {ip}", tcpIp);
    }

    var tcpPort = Environment.GetEnvironmentVariable("QALITRACK_TCP_PORT");
    if (!string.IsNullOrWhiteSpace(tcpPort) && int.TryParse(tcpPort, out int port))
    {
        config.TcpListener.Port = port;
        logger.LogInformation("ENV override — TCP Port: {port}", port);
    }

    var rfidEnabled = Environment.GetEnvironmentVariable("QALITRACK_RFID_ENABLED");
    if (!string.IsNullOrWhiteSpace(rfidEnabled) && bool.TryParse(rfidEnabled, out bool enabled))
    {
        config.RfidSettings.Enabled = enabled;
        logger.LogInformation("ENV override — RFID Enabled: {enabled}", enabled);
    }

    var rfidHost = Environment.GetEnvironmentVariable("QALITRACK_RFID_HOST");
    if (!string.IsNullOrWhiteSpace(rfidHost))
    {
        config.RfidSettings.Host = rfidHost;
        logger.LogInformation("ENV override — RFID Host: {host}", rfidHost);
    }

    var rfidPort = Environment.GetEnvironmentVariable("QALITRACK_RFID_PORT");
    if (!string.IsNullOrWhiteSpace(rfidPort) && int.TryParse(rfidPort, out int rport))
    {
        config.RfidSettings.Port = rport;
        logger.LogInformation("ENV override — RFID Port: {port}", rport);
    }
}