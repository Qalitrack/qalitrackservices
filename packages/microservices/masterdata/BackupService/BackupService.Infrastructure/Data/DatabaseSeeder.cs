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
        // Ensure database is created
        await _context.Database.EnsureCreatedAsync(cancellationToken);

        // Check if microservices already exist to avoid duplicates
        if (!await _context.Microservices.AnyAsync(cancellationToken))
        {
            var microservices = new List<Microservice>
            {
                new Microservice
                {
                    Name = "AuthService",
                    ConnectionString = "postgres://user:password@localhost:5432/auth_db", // Placeholder, encrypt in production
                    Status = MicroserviceStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Microservice
                {
                    Name = "OrderService",
                    ConnectionString = "postgres://user:password@localhost:5432/orders_db", // Placeholder, encrypt in production
                    Status = MicroserviceStatus.Paused, // Example: Paused for maintenance
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Microservice
                {
                    Name = "UserService",
                    ConnectionString = "Host=postgres-db;Port=5432;Database=userservicedb;Username=userservice;Password=userservice123;MinPoolSize=50;MaxPoolSize=500;Timeout=30;CommandTimeout=60;ConnectionIdleLifetime=300;ConnectionPruningInterval=10;Pooling=true;",
                    Status = MicroserviceStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            _context.Microservices.AddRange(microservices);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

}
    