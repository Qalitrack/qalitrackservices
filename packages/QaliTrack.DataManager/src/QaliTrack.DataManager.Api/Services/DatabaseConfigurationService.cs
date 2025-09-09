using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.AuthMethods;

namespace QaliTrack.DataManager.Api.Services;

public interface IDatabaseConfigurationService
{
    Task<string> GetConnectionStringAsync();
    string GetDatabaseProvider();
}

public class DatabaseConfigurationService : IDatabaseConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseConfigurationService> _logger;

    public DatabaseConfigurationService(IConfiguration configuration, ILogger<DatabaseConfigurationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string GetDatabaseProvider()
    {
        // Check environment variable first, then config, default to SQLite
        var provider = Environment.GetEnvironmentVariable("DATABASE_PROVIDER") 
                      ?? _configuration["Database:Provider"] 
                      ?? "SQLite";
        
        _logger.LogInformation("Using database provider: {Provider}", provider);
        return provider;
    }

    public async Task<string> GetConnectionStringAsync()
    {
        var provider = GetDatabaseProvider();
        
        return provider.ToUpper() switch
        {
            "SQLITE" => GetSQLiteConnectionString(),
            "POSTGRESQL" or "POSTGRES" => await GetPostgreSQLConnectionStringAsync(),
            _ => throw new NotSupportedException($"Database provider '{provider}' is not supported.")
        };
    }

    private string GetSQLiteConnectionString()
    {
        var dbPath = Environment.GetEnvironmentVariable("SQLITE_DB_PATH") 
                    ?? _configuration["Database:SQLite:Path"] 
                    ?? "Data/datamanager.db";

        // Ensure directory exists
        var directory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            _logger.LogInformation("Created database directory: {Directory}", directory);
        }
        
        var connectionString = $"Data Source={dbPath}";
        _logger.LogInformation("SQLite connection string created for: {DbPath}", dbPath);
        return connectionString;
    }

    private async Task<string> GetPostgreSQLConnectionStringAsync()
    {
        // Strategy: Try environment first, then vault if credentials are incomplete
        _logger.LogInformation("Attempting to configure PostgreSQL connection...");
        
        // Step 1: Try to get credentials from environment/config
        var envResult = TryGetPostgreSQLFromEnvironment();
        if (envResult.Success)
        {
            _logger.LogInformation("PostgreSQL credentials successfully retrieved from environment");
            return envResult.ConnectionString;
        }
        
        _logger.LogWarning("Environment credentials incomplete: {Reason}. Attempting Vault fallback...", envResult.ErrorMessage);
        
        // Step 2: Try to get credentials from Vault as fallback
        var vaultResult = await TryGetPostgreSQLFromVaultAsync();
        if (vaultResult.Success)
        {
            _logger.LogInformation("PostgreSQL credentials successfully retrieved from Vault");
            return vaultResult.ConnectionString;
        }
        
        // Step 3: Both methods failed, fallback to SQLite as ultimate fallback
        _logger.LogError("Failed to retrieve PostgreSQL credentials from both environment and Vault. " +
                        "Environment error: {EnvError}. Vault error: {VaultError}. " +
                        "Falling back to SQLite for graceful degradation.", 
                        envResult.ErrorMessage, vaultResult.ErrorMessage);
        
        _logger.LogWarning("FALLBACK: Using SQLite instead of PostgreSQL due to missing credentials. " +
                          "To use PostgreSQL, provide complete credentials via environment variables or Vault.");
        
        return GetSQLiteConnectionString();
    }

    private (bool Success, string ConnectionString, string ErrorMessage) TryGetPostgreSQLFromEnvironment()
    {
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") 
                  ?? _configuration["Database:PostgreSQL:Host"] 
                  ?? "localhost";
        
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") 
                  ?? _configuration["Database:PostgreSQL:Port"] 
                  ?? "5432";
        
        var database = Environment.GetEnvironmentVariable("POSTGRES_DATABASE") 
                      ?? _configuration["Database:PostgreSQL:Database"] 
                      ?? "qalitrack_datamanager";
        
        var username = Environment.GetEnvironmentVariable("POSTGRES_USERNAME") 
                      ?? _configuration["Database:PostgreSQL:Username"] 
                      ?? "postgres";
        
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") 
                      ?? _configuration["Database:PostgreSQL:Password"];

        // Validate that all required credentials are present and non-empty
        var validationError = ValidatePostgreSQLCredentials(host, port, database, username, password);
        if (validationError != null)
        {
            return (false, string.Empty, validationError);
        }

        var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
        _logger.LogInformation("PostgreSQL connection string created from environment for {Host}:{Port}/{Database}", host, port, database);
        
        return (true, connectionString, string.Empty);
    }

    private async Task<(bool Success, string ConnectionString, string ErrorMessage)> TryGetPostgreSQLFromVaultAsync()
    {
        try
        {
            var vaultUrl = Environment.GetEnvironmentVariable("VAULT_URL") 
                          ?? _configuration["Vault:Url"] 
                          ?? "http://localhost:8200";
            
            var vaultToken = Environment.GetEnvironmentVariable("VAULT_TOKEN")
                           ?? _configuration["Vault:Token"];
            
            if (string.IsNullOrEmpty(vaultToken))
            {
                return (false, string.Empty, "VAULT_TOKEN environment variable or Vault:Token configuration is required");
            }

            var authMethod = new TokenAuthMethodInfo(vaultToken);
            var vaultClientSettings = new VaultClientSettings(vaultUrl, authMethod);
            var vaultClient = new VaultClient(vaultClientSettings);

            var secretPath = GetVaultSecretPath();
            _logger.LogInformation("Reading PostgreSQL credentials from Vault at {VaultUrl} path: {SecretPath}", vaultUrl, secretPath);

            var secret = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(secretPath);
            var dbConfig = secret.Data.Data;

            var host = dbConfig.TryGetValue("host", out var h) ? h.ToString() : "localhost";
            var port = dbConfig.TryGetValue("port", out var p) ? p.ToString() : "5432";
            var database = dbConfig.TryGetValue("database", out var db) ? db.ToString() : "qalitrack_datamanager";
            var username = dbConfig.TryGetValue("username", out var u) ? u.ToString() : "postgres";
            var password = dbConfig.TryGetValue("password", out var pw) ? pw.ToString() : null;

            // Validate that all required credentials are present and non-empty
            var validationError = ValidatePostgreSQLCredentials(host, port, database, username, password);
            if (validationError != null)
            {
                return (false, string.Empty, $"Vault credentials incomplete: {validationError}");
            }

            var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
            _logger.LogInformation("PostgreSQL connection string retrieved from Vault for {Host}:{Port}/{Database}", host, port, database);
            
            return (true, connectionString, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, string.Empty, $"Vault access failed: {ex.Message}");
        }
    }

    private string? ValidatePostgreSQLCredentials(string host, string port, string database, string username, string? password)
    {
        if (string.IsNullOrWhiteSpace(host))
            return "POSTGRES_HOST is required";
        
        if (string.IsNullOrWhiteSpace(port))
            return "POSTGRES_PORT is required";
        
        if (string.IsNullOrWhiteSpace(database))
            return "POSTGRES_DATABASE is required";
        
        if (string.IsNullOrWhiteSpace(username))
            return "POSTGRES_USERNAME is required";
        
        if (string.IsNullOrWhiteSpace(password))
            return "POSTGRES_PASSWORD is required and cannot be empty";
        
        // Validate port is numeric
        if (!int.TryParse(port, out var portNumber) || portNumber <= 0 || portNumber > 65535)
            return "POSTGRES_PORT must be a valid port number (1-65535)";
        
        return null; // All validations passed
    }

    private string GetVaultSecretPath()
    {
        return Environment.GetEnvironmentVariable("VAULT_DB_SECRET_PATH") 
               ?? _configuration["Vault:DatabaseSecretPath"] 
               ?? "secret/data/database/postgresql";
    }
}