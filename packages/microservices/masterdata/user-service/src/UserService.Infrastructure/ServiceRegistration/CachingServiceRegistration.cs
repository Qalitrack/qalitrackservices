using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using UserService.Core.Interfaces;
using UserService.Core.Services;
using UserService.Infrastructure.Services;
using StackExchange.Redis;
using UserService.Core.Interfaces.Services;

namespace UserService.Infrastructure.ServiceRegistration;

public static class CachingServiceRegistration
{
    public static IServiceCollection AddCachingServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var useRedis = configuration.GetValue<bool>("UseRedis", false);
        var redisConnectionString = configuration.GetConnectionString("Redis");

        if (useRedis && !string.IsNullOrEmpty(redisConnectionString))
        {
            // Configure Redis for distributed caching (1000+ users)
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "UserService";
            });

            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(redisConnectionString)
            );

            services.AddScoped<ICacheService, RedisCacheService>();

            Serilog.Log.Information("Redis distributed cache configured for enterprise scale (1000+ users)");
        }
        else
        {
            // Configure in-memory caching for smaller deployments
            services.AddMemoryCache();
            services.AddScoped<ICacheService, MemoryCacheService>();

            Serilog.Log.Information("Memory cache configured (suitable for <500 users)");
        }

        return services;
    }
}
