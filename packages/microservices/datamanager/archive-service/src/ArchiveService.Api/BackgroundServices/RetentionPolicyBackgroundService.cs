using ArchiveService.Core.Interfaces;

namespace ArchiveService.Api.BackgroundServices
{
    public class RetentionPolicyBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RetentionPolicyBackgroundService> _logger;
        private readonly TimeSpan _executionInterval = TimeSpan.FromHours(1); // Execute every hour

        public RetentionPolicyBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<RetentionPolicyBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Retention Policy Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExecuteRetentionPoliciesAsync(stoppingToken);
                    await Task.Delay(_executionInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Retention Policy Background Service is stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in Retention Policy Background Service");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait 5 minutes before retrying
                }
            }

            _logger.LogInformation("Retention Policy Background Service stopped");
        }

        private async Task ExecuteRetentionPoliciesAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var policyEngine = scope.ServiceProvider.GetRequiredService<IRetentionPolicyEngine>();

            try
            {
                _logger.LogInformation("Starting retention policy execution cycle");

                var result = await policyEngine.ExecuteAllPoliciesAsync(cancellationToken);
                
                if (result)
                {
                    _logger.LogInformation("Retention policy execution cycle completed successfully");
                }
                else
                {
                    _logger.LogWarning("Retention policy execution cycle completed with some failures");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing retention policies");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retention Policy Background Service is stopping...");
            await base.StopAsync(cancellationToken);
        }
    }
}