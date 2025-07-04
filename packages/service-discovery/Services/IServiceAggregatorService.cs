using ServiceDiscovery.Models;

namespace ServiceDiscovery.Services;

public interface IServiceAggregatorService
{
    Task<ServiceDiscoveryResponse> GetAllServicesAsync();
    Task<string> GetServicesHtmlAsync();
}