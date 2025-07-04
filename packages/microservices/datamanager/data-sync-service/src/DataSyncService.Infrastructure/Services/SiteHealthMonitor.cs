using Microsoft.Extensions.Logging;
using DataSyncService.Core.DTOs;
using DataSyncService.Core.Entities;
using DataSyncService.Core.Enums;
using DataSyncService.Core.Interfaces;
using System.Diagnostics;
using System.Net.Http;

namespace DataSyncService.Infrastructure.Services;

public class SiteHealthMonitor : ISiteHealthMonitor
{
    private readonly ISyncSiteRepository _syncSiteRepository;
    private readonly ILogger<SiteHealthMonitor> _logger;
    private readonly HttpClient _httpClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Timer? _monitoringTimer;

    public SiteHealthMonitor(
        ISyncSiteRepository syncSiteRepository,
        ILogger<SiteHealthMonitor> logger,
        HttpClient httpClient,
        IUnitOfWork unitOfWork)
    {
        _syncSiteRepository = syncSiteRepository;
        _logger = logger;
        _httpClient = httpClient;
        _unitOfWork = unitOfWork;
    }

    public async Task<HealthCheckDto> CheckSiteHealthAsync(string siteId)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Checking health for site {SiteId}", siteId);

            var site = await _syncSiteRepository.GetBySiteIdAsync(siteId);
            if (site == null)
            {
                return new HealthCheckDto
                {
                    SiteId = siteId,
                    Status = "NotFound",
                    CheckTime = DateTime.UtcNow,
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                    ErrorMessage = "Site not found",
                    IsHealthy = false
                };
            }

            var healthCheck = new HealthCheckDto
            {
                SiteId = siteId,
                CheckTime = DateTime.UtcNow,
                IsHealthy = true,
                Status = "Healthy"
            };

            // Perform multiple health checks
            var pingResult = await PingSiteAsync(siteId);
            var apiResult = await CheckApiEndpointAsync(siteId);
            var dbResult = await CheckDatabaseConnectionAsync(siteId);

            // Determine overall health
            healthCheck.IsHealthy = pingResult && apiResult && dbResult;
            healthCheck.Status = healthCheck.IsHealthy ? "Healthy" : "Unhealthy";
            healthCheck.ResponseTimeMs = stopwatch.ElapsedMilliseconds;

            if (!healthCheck.IsHealthy)
            {
                var errors = new List<string>();
                if (!pingResult) errors.Add("Ping failed");
                if (!apiResult) errors.Add("API endpoint unreachable");
                if (!dbResult) errors.Add("Database connection failed");
                
                healthCheck.ErrorMessage = string.Join("; ", errors);
            }

            // Update site status
            var newStatus = healthCheck.IsHealthy ? SiteStatus.Active : SiteStatus.Error;
            await _syncSiteRepository.UpdateSiteStatusAsync(siteId, newStatus);

            // Record health check
            await RecordHealthCheckAsync(site, healthCheck);

            _logger.LogInformation("Health check completed for site {SiteId}: {Status} ({ResponseTime}ms)", 
                siteId, healthCheck.Status, healthCheck.ResponseTimeMs);

            return healthCheck;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking health for site {SiteId}", siteId);
            
            stopwatch.Stop();
            return new HealthCheckDto
            {
                SiteId = siteId,
                Status = "Error",
                CheckTime = DateTime.UtcNow,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                ErrorMessage = ex.Message,
                IsHealthy = false
            };
        }
    }

    public async Task<IEnumerable<HealthCheckDto>> CheckAllSitesHealthAsync()
    {
        try
        {
            _logger.LogInformation("Checking health for all sites");

            var sites = await _syncSiteRepository.GetActiveSites();
            var healthChecks = new List<HealthCheckDto>();

            var tasks = sites.Select(site => CheckSiteHealthAsync(site.SiteId));
            var results = await Task.WhenAll(tasks);
            
            healthChecks.AddRange(results);

            _logger.LogInformation("Completed health checks for {SiteCount} sites", sites.Count());

            return healthChecks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking health for all sites");
            return new List<HealthCheckDto>();
        }
    }

    public async Task<bool> IsSiteHealthyAsync(string siteId)
    {
        try
        {
            var healthCheck = await CheckSiteHealthAsync(siteId);
            return healthCheck.IsHealthy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if site {SiteId} is healthy", siteId);
            return false;
        }
    }

    public async Task StartMonitoringAsync()
    {
        try
        {
            _logger.LogInformation("Starting site health monitoring");

            // Create a timer that runs every 5 minutes
            var timer = new Timer(async _ => await PerformPeriodicHealthChecksAsync(), 
                null, TimeSpan.Zero, TimeSpan.FromMinutes(5));

            _logger.LogInformation("Site health monitoring started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting site health monitoring");
        }
    }

    public async Task StopMonitoringAsync()
    {
        try
        {
            _monitoringTimer?.Dispose();
            _logger.LogInformation("Site health monitoring stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping site health monitoring");
        }
    }

    public async Task<bool> PingSiteAsync(string siteId)
    {
        try
        {
            var site = await _syncSiteRepository.GetBySiteIdAsync(siteId);
            if (site == null) return false;

            // Extract host from API endpoint
            var uri = new Uri(site.ApiEndpoint);
            var host = uri.Host;

            // Simple ping using HTTP request (since ICMP ping requires elevated privileges)
            using var response = await _httpClient.GetAsync($"http://{host}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Ping failed for site {SiteId}", siteId);
            return false;
        }
    }

    public async Task<bool> CheckDatabaseConnectionAsync(string siteId)
    {
        try
        {
            var site = await _syncSiteRepository.GetBySiteIdAsync(siteId);
            if (site == null) return false;

            // In a real implementation, you would:
            // 1. Parse the connection string
            // 2. Create a connection to the database
            // 3. Execute a simple query (SELECT 1)
            // 4. Return true if successful

            // For now, simulate database check
            await Task.Delay(100);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Database connection check failed for site {SiteId}", siteId);
            return false;
        }
    }

    public async Task<bool> CheckApiEndpointAsync(string siteId)
    {
        try
        {
            var site = await _syncSiteRepository.GetBySiteIdAsync(siteId);
            if (site == null) return false;

            // Check if API endpoint is reachable
            var healthEndpoint = $"{site.ApiEndpoint.TrimEnd('/')}/health";
            
            using var response = await _httpClient.GetAsync(healthEndpoint);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "API endpoint check failed for site {SiteId}", siteId);
            return false;
        }
    }

    private async Task PerformPeriodicHealthChecksAsync()
    {
        try
        {
            _logger.LogDebug("Performing periodic health checks");

            var sitesRequiringCheck = await _syncSiteRepository.GetSitesRequiringHealthCheckAsync();
            
            foreach (var site in sitesRequiringCheck)
            {
                await CheckSiteHealthAsync(site.SiteId);
                
                // Small delay between checks to avoid overwhelming sites
                await Task.Delay(1000);
            }

            _logger.LogDebug("Completed periodic health checks for {SiteCount} sites", sitesRequiringCheck.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing periodic health checks");
        }
    }

    private async Task RecordHealthCheckAsync(SyncSite site, HealthCheckDto healthCheck)
    {
        try
        {
            var healthCheckEntity = new SiteHealthCheck
            {
                HealthCheckId = Guid.NewGuid().ToString(),
                SiteId = site.SiteId,
                Status = healthCheck.IsHealthy ? SiteStatus.Active : SiteStatus.Error,
                CheckTime = healthCheck.CheckTime,
                ResponseTimeMs = healthCheck.ResponseTimeMs,
                ErrorMessage = healthCheck.ErrorMessage,
                Details = healthCheck.Status,
                CheckType = "Automatic",
                IsHealthy = healthCheck.IsHealthy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            site.HealthChecks.Add(healthCheckEntity);
            site.LastHealthCheck = DateTime.UtcNow;
            
            await _syncSiteRepository.UpdateAsync(site);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording health check for site {SiteId}", site.SiteId);
        }
    }
}