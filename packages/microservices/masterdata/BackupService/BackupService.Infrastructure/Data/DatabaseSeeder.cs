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
                    Name = "UserService",
                    ConnectionString = "Host=postgres-userservice-prod;Port=5432;Database=userservicedb;Username=userservice;Password=userservice123;MinPoolSize=50;MaxPoolSize=500;Timeout=30;CommandTimeout=60;ConnectionIdleLifetime=300;ConnectionPruningInterval=10;Pooling=true;",
                    Status = MicroserviceStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Microservice
                {
                    Name = "MasterDataService",
                    ConnectionString = "Host=postgres-masterdata-prod;Port=5432;Database=qalitrack_masterdata;Username=masterdata;Password=masterdata123;MinPoolSize=50;MaxPoolSize=500;Timeout=30;CommandTimeout=60;ConnectionIdleLifetime=300;ConnectionPruningInterval=10;Pooling=true;",
                    Status = MicroserviceStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Microservice
                {
                    Name = "BackupService",
                    ConnectionString = "Host=postgres-backupservice-prod;Port=5432;Database=backupservicedb;Username=backupservice;Password=backupservice123;MinPoolSize=50;MaxPoolSize=500;Timeout=30;CommandTimeout=60;ConnectionIdleLifetime=300;ConnectionPruningInterval=10;Pooling=true;",
                    Status = MicroserviceStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Microservice
                {
                    Name = "TransactionService",
                    ConnectionString = "Host=postgres-transaction-prod;Port=5432;Database=transactiondb;Username=transaction;Password=transaction123;MinPoolSize=50;MaxPoolSize=500;Timeout=30;CommandTimeout=60;ConnectionIdleLifetime=300;ConnectionPruningInterval=10;Pooling=true;",
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
    