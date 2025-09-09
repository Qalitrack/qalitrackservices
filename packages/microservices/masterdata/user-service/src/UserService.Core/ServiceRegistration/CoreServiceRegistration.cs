using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserService.Core.Services;
using UserService.Core.Interfaces.Services;

namespace UserService.Core.ServiceRegistration;

public static class CoreServiceRegistration
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // Core Services - Register base service first, then decorate with caching
        services.AddScoped<UserService.Core.Services.UserService>();
        services.AddScoped<IUserService>(provider =>
        {
            var baseUserService = provider.GetRequiredService<UserService.Core.Services.UserService>();
            var cacheService = provider.GetRequiredService<ICacheService>();
            var logger = provider.GetRequiredService<ILogger<UserService.Core.Services.CachedUserService>>();
            return new CachedUserService(baseUserService, cacheService, logger);
        });

        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<IUserRoleService, UserRoleService>();
        services.AddScoped<IPermissionsService, PermissionsService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserStatusService, UserStatusService>();
        services.AddScoped<ITwoFactorService, TwoFactorService>();
        services.AddScoped<PasswordPolicyService>();
        services.AddScoped<IJwtConfigurationService, JwtConfigurationService>();

        return services;
    }
}
