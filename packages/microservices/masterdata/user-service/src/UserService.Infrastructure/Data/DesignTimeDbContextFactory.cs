using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace UserService.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<UserServiceDbContext>
{
    public UserServiceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UserServiceDbContext>();
        
        // Try to find the correct base path for configuration files
        var basePath = FindConfigurationBasePath();
        
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables();

        var configuration = configurationBuilder.Build();

        // Get database configuration
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var usePostgreSql = configuration.GetValue<bool>("UsePostgreSQL", false);

        // Configure the appropriate database provider
        if (usePostgreSql && !string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine($"Using PostgreSQL with connection: {MaskConnectionString(connectionString)}");
            optionsBuilder.UseNpgsql(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly("UserService.Infrastructure");
                sqlOptions.CommandTimeout(30);
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
            });
        }
        else
        {
            // Default to SQLite for design-time operations
            var sqliteConnectionString = connectionString ?? "Data Source=user-service-design.db";
            Console.WriteLine($"Using SQLite with connection: {sqliteConnectionString}");
            optionsBuilder.UseSqlite(sqliteConnectionString, sqlOptions => 
            {
                sqlOptions.MigrationsAssembly("UserService.Infrastructure");
                sqlOptions.CommandTimeout(30);
            });
        }

        // Configure for design-time
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        optionsBuilder.EnableSensitiveDataLogging(false);
        
        return new UserServiceDbContext(optionsBuilder.Options);
    }

    private static string FindConfigurationBasePath()
    {
        var currentPath = Directory.GetCurrentDirectory();
        Console.WriteLine($"Current directory: {currentPath}");

        // Common paths to check for appsettings.json
        var pathsToCheck = new[]
        {
            currentPath,
            Path.Combine(currentPath, "../UserService.Api"),
            Path.Combine(currentPath, "../../UserService.Api"),
            Path.Combine(currentPath, "src/UserService.Api"),
            Path.Combine(currentPath, "../src/UserService.Api"),
            Path.Combine(currentPath, "../../src/UserService.Api")
        };

        foreach (var path in pathsToCheck)
        {
            var fullPath = Path.GetFullPath(path);
            var appsettingsPath = Path.Combine(fullPath, "appsettings.json");
            
            if (File.Exists(appsettingsPath))
            {
                Console.WriteLine($"Found appsettings.json at: {fullPath}");
                return fullPath;
            }
        }

        Console.WriteLine($"Could not find appsettings.json, using current directory: {currentPath}");
        return currentPath;
    }

    private static string MaskConnectionString(string connectionString)
    {
        // Simple masking for security - replace password values
        return System.Text.RegularExpressions.Regex.Replace(
            connectionString, 
            @"(password|pwd)=([^;]+)", 
            "$1=***", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
}