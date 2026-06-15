using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BackupService.Infrastructure.Data;

public class BackupServiceDbContextFactory : IDesignTimeDbContextFactory<BackupServiceDbContext>
{
    public BackupServiceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BackupServiceDbContext>();
        optionsBuilder.UseNpgsql(
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? "Host=localhost;Port=5432;Database=qalitrackdb;Username=qalitrack;Password=qalitrack123",
            npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "backup"));
        return new BackupServiceDbContext(optionsBuilder.Options);
    }
}