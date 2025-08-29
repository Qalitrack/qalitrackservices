using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.Interfaces.Services;
using UserService.Core.Interfaces.Emails;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Repositories;
using UserService.Infrastructure.Services;
using UserService.Core.Mappings;
using System.IO.Abstractions;
using StackExchange.Redis;
using UserService.Core.Configuration;

namespace UserService.Infrastructure.ServiceRegistration;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Email services
        services.AddSingleton<EmailQueueService>();
        services.AddScoped<IEmailQueueService>(provider => provider.GetRequiredService<EmailQueueService>());
        services.AddScoped<IEmailService, SmtpEmailService>();

        // File system abstraction
        services.AddSingleton<IFileSystem, FileSystem>();

        // HTTP context
        services.AddHttpContextAccessor();

        // Health checks
        services.AddHealthChecks();

        // Caching services
        RegisterCachingServices(services, configuration);

        // Repository registrations
        RegisterRepositories(services);

        // Background services
        RegisterBackgroundServices(services);

        // Infrastructure services
        RegisterInfrastructureServices(services);

        return services;
    }

    private static void RegisterInfrastructureServices(IServiceCollection services)
    {
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IShiftLoginRestrictionService, ShiftLoginRestrictionService>();
    }

    private static void RegisterCachingServices(IServiceCollection services, IConfiguration configuration)
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
    }

    private static void RegisterRepositories(IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IShiftRepository, ShiftRepository>();
        services.AddScoped<IUserShiftRepository, UserShiftRepository>();
        services.AddScoped<ITokenRepository, TokenRepository>();
        services.AddScoped<IPermissionsRepository, PermissionsRepository>();
        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        services.AddScoped<IUserStatusRepository, UserStatusRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IPasswordPolicyRepository, PasswordPolicyRepository>();
    }

    private static void RegisterBackgroundServices(IServiceCollection services)
    {
        services.AddHostedService<ShiftMonitorService>();
        services.AddHostedService<EmailProcessorService>();
    }
}
