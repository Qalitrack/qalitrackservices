using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Qalitrack.Services;

/// <summary>
/// Wraps an IDisposable service to ensure proper cleanup when the application shuts down.
/// </summary>
/// <typeparam name="T">The type of service to wrap</typeparam>
public class BackgroundServiceWrapper<T> : BackgroundService where T : IDisposable
{
    private readonly T _service;
    private readonly ILogger<BackgroundServiceWrapper<T>> _logger;

    public BackgroundServiceWrapper(T service, ILogger<BackgroundServiceWrapper<T>> logger = null)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Keep the service running until cancellation is requested
        try
        {
            _logger?.LogInformation("Started background service wrapper for {Type}", typeof(T).Name);
            
            // Wait until cancellation is requested
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
            _logger?.LogInformation("Background service wrapper for {Type} is stopping", typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in background service wrapper for {Type}", typeof(T).Name);
            throw;
        }
    }

    public override void Dispose()
    {
        try
        {
            _service?.Dispose();
            _logger?.LogInformation("Disposed background service wrapper for {Type}", typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error disposing service {Type}", typeof(T).Name);
        }
        
        base.Dispose();
    }
}
