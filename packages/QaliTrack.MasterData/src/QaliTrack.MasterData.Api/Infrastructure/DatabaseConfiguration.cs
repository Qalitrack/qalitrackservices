using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Infrastructure.Data;

namespace QaliTrack.MasterData.Api.Infrastructure;

public static class DatabaseConfiguration
{
    public static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        // Load environment variables from .env file (Django-style)
        DotNetEnv.Env.Load();

        var dbProvider = Environment.GetEnvironmentVariable("DB_PROVIDER") ?? "SQLite";
        var connectionString = BuildConnectionString(dbProvider);

        switch (dbProvider.ToUpper())
        {
            case "POSTGRESQL":
                services.AddDbContext<MasterDataDbContext>(options =>
                    options.UseNpgsql(connectionString));
                break;
            
            case "SQLITE":
            default:
                services.AddDbContext<MasterDataDbContext>(options =>
                    options.UseSqlite(connectionString));
                break;
        }
    }

    private static string BuildConnectionString(string provider)
    {
        return provider.ToUpper() switch
        {
            "POSTGRESQL" => BuildPostgreSQLConnectionString(),
            "SQLITE" => BuildSQLiteConnectionString(),
            _ => BuildSQLiteConnectionString() // Default to SQLite
        };
    }

    private static string BuildPostgreSQLConnectionString()
    {
        var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
        var database = Environment.GetEnvironmentVariable("DB_NAME") ?? "masterdata";
        var username = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

        return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    }

    private static string BuildSQLiteConnectionString()
    {
        var dbPath = Environment.GetEnvironmentVariable("DB_PATH") ?? "masterdata.db";
        return $"Data Source={dbPath}";
    }
}