using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace UserService.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<UserServiceDbContext>
{
    public UserServiceDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Could not find a connection string named 'DefaultConnection'");
        }

        Console.WriteLine($"Using connection string: {MaskConnectionString(connectionString)}");
        
        var optionsBuilder = new DbContextOptionsBuilder<UserServiceDbContext>();
        optionsBuilder.UseNpgsql(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly("UserService.Infrastructure");
            sqlOptions.CommandTimeout(30);
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null
            );
            sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "users");
        });

        return new UserServiceDbContext(optionsBuilder.Options);
    }

    private static string MaskConnectionString(string connectionString)
    {
        // Mask sensitive information in the connection string for logging
        return connectionString
            .Replace("User ID=", "User ID=***")
            .Replace("Password=", "Password=***")
            .Replace("Username=", "Username=***");
    }
}