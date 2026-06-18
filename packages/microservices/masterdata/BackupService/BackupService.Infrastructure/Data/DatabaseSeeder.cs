using BackupService.Core.Entities;
using BackupService.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace BackupService.Infrastructure.Data;

public class DatabaseSeeder
{
    private readonly BackupServiceDbContext _context;

    public DatabaseSeeder(BackupServiceDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.EnsureCreatedAsync(cancellationToken);

        // Remove any stale entries that aren't QalitrackDB
        var stale = await _context.Microservices
            .Where(m => m.Name != "QalitrackDB")
            .ToListAsync(cancellationToken);
        if (stale.Count > 0)
            _context.Microservices.RemoveRange(stale);

        const string connString = "Host=postgres-prod;Port=5432;Database=qalitrackdb;Username=qalitrack;Password=qalitrack123;MinPoolSize=50;MaxPoolSize=500;Timeout=30;CommandTimeout=60;ConnectionIdleLifetime=300;ConnectionPruningInterval=10;Pooling=true;";

        var existing = await _context.Microservices.FirstOrDefaultAsync(m => m.Name == "QalitrackDB", cancellationToken);
        if (existing == null)
        {
            _context.Microservices.Add(new Microservice
            {
                Name = "QalitrackDB",
                ConnectionString = connString,
                Status = MicroserviceStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.ConnectionString = connString;
            existing.Status = MicroserviceStatus.Active;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

}
    