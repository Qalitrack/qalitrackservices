using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.AppRole;
using VaultSharp.V1.Commons;

namespace QaliTrack.Services.Vault
{
    public class VaultConfiguration
    {
        public string Address { get; set; } = "http://localhost:8200";
        public string RoleName { get; set; } = "qalitrack-services";
        public string RoleId { get; set; } = "";
        public string SecretId { get; set; } = "";
    }

    public class VaultService
    {
        private readonly IVaultClient _vaultClient;
        private readonly ILogger<VaultService> _logger;

        public VaultService(IVaultClient vaultClient, ILogger<VaultService> logger)
        {
            _vaultClient = vaultClient;
            _logger = logger;
        }

        public async Task<string> GetSecretAsync(string path)
        {
            try
            {
                var secret = await _vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(path);
                return secret.Data.Data.First().Value.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve secret from path: {Path}", path);
                throw;
            }
        }

        public async Task<Dictionary<string, object>> GetSecretsAsync(string path)
        {
            try
            {
                var secret = await _vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(path);
                return secret.Data.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve secrets from path: {Path}", path);
                throw;
            }
        }
    }

    public static class VaultServiceExtensions
    {
        public static IServiceCollection AddVaultIntegration(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            var vaultConfig = configuration.GetSection("Vault").Get<VaultConfiguration>();
            
            if (vaultConfig == null || string.IsNullOrEmpty(vaultConfig.Address))
            {
                // If Vault is not configured, skip registration
                return services;
            }

            services.Configure<VaultConfiguration>(configuration.GetSection("Vault"));

            services.AddSingleton<IVaultClient>(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<VaultService>>();
                
                try
                {
                    // Try to get role credentials from Vault first
                    var roleId = vaultConfig.RoleId;
                    var secretId = vaultConfig.SecretId;

                    if (string.IsNullOrEmpty(roleId) || string.IsNullOrEmpty(secretId))
                    {
                        // Try to get from environment or use pre-stored values
                        roleId = Environment.GetEnvironmentVariable("VAULT_ROLE_ID") ?? "";
                        secretId = Environment.GetEnvironmentVariable("VAULT_SECRET_ID") ?? "";
                    }

                    if (string.IsNullOrEmpty(roleId) || string.IsNullOrEmpty(secretId))
                    {
                        logger.LogWarning("Vault role ID or secret ID not configured. Using root token for development.");
                        // Fall back to root token for development
                        var authMethod = new TokenAuthMethodInfo("myroot");
                        var vaultClientSettings = new VaultClientSettings(vaultConfig.Address, authMethod);
                        return new VaultClient(vaultClientSettings);
                    }

                    var appRoleAuthMethod = new AppRoleAuthMethodInfo(vaultConfig.RoleName, roleId, secretId);
                    var settings = new VaultClientSettings(vaultConfig.Address, appRoleAuthMethod);
                    return new VaultClient(settings);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to initialize Vault client");
                    throw;
                }
            });

            services.AddScoped<VaultService>();
            return services;
        }
    }

    // Configuration Provider for Vault
    public class VaultConfigurationProvider : ConfigurationProvider
    {
        private readonly VaultService _vaultService;
        private readonly string[] _secretPaths;

        public VaultConfigurationProvider(VaultService vaultService, string[] secretPaths)
        {
            _vaultService = vaultService;
            _secretPaths = secretPaths;
        }

        public override void Load()
        {
            LoadAsync().GetAwaiter().GetResult();
        }

        private async Task LoadAsync()
        {
            Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var path in _secretPaths)
            {
                try
                {
                    var secrets = await _vaultService.GetSecretsAsync(path);
                    
                    foreach (var secret in secrets)
                    {
                        var key = $"{path.Replace("/", ":")}:{secret.Key}";
                        Data[key] = secret.Value.ToString();
                    }
                }
                catch (Exception ex)
                {
                    // Log but don't fail startup
                    Console.WriteLine($"Failed to load secrets from {path}: {ex.Message}");
                }
            }
        }
    }
}

/*
REQUIRED NUGET PACKAGES:
- VaultSharp (>= 1.13.0.1)

USAGE IN STARTUP.CS / PROGRAM.CS:

// Add to ConfigureServices/Program.cs
services.AddVaultIntegration(configuration);

// Example usage in a service:
public class MyService
{
    private readonly VaultService _vaultService;
    
    public MyService(VaultService vaultService)
    {
        _vaultService = vaultService;
    }
    
    public async Task<string> GetDatabaseConnectionString()
    {
        var secrets = await _vaultService.GetSecretsAsync("database/masterdata");
        return secrets["connection_string"].ToString();
    }
}

DOCKER ENVIRONMENT VARIABLES NEEDED:
- VAULT__ADDRESS=http://vault:8200
- VAULT__ROLENAME=qalitrack-services
- VAULT_ROLE_ID=<obtained from vault-init script>
- VAULT_SECRET_ID=<obtained from vault-init script>
*/