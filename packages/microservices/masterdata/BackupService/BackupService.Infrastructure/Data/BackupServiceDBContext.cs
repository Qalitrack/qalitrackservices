using BackupService.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackupService.Infrastructure.Data;

public class BackupServiceDbContext : DbContext
{
    public BackupServiceDbContext(DbContextOptions<BackupServiceDbContext> options)
        : base(options)
    {
    }

    public DbSet<BackupOperationRecord> BackupOperations { get; set; }
    public DbSet<BackupServiceResponseRecord> ServiceResponses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BackupOperationRecord>()
            .HasKey(o => o.CommandId);

        modelBuilder.Entity<BackupServiceResponseRecord>()
            .HasKey(r => r.Id);

        modelBuilder.Entity<BackupServiceResponseRecord>()
            .HasOne<BackupOperationRecord>()
            .WithMany(o => o.ServiceResponses)
            .HasForeignKey(r => r.CommandId);
    }
}