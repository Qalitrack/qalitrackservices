using Microsoft.Extensions.Diagnostics.HealthChecks;
namespace UserService.Core.Interfaces
{
    public interface IHealthCheckService
    {
        Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default);
    }
}