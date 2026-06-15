using BackupService.Core.Entities;
using BackupService.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace BackupService.Infrastructure.Data;

public class BackupServiceDbContext : DbContext
{
    public BackupServiceDbContext(DbContextOptions<BackupServiceDbContext> options)
        : base(options)
    {
        
    }

   public DbSet<Microservice> Microservices { get; set; }
   public DbSet<BackupChain> BackupChains { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("backup");
            // Configure Microservices table
            modelBuilder.Entity<Microservice>()
                .ToTable("microservices", "backup")
                .HasKey(m => m.Id);

            modelBuilder.Entity<Microservice>()
                .Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Microservice>()
                .HasIndex(m => m.Name)
                .IsUnique();

            modelBuilder.Entity<Microservice>()
                .Property(m => m.ConnectionString)
                .IsRequired();

            modelBuilder.Entity<Microservice>()
                .Property(m => m.Status)
                .HasConversion<string>()
                .HasDefaultValue(MicroserviceStatus.Active)
                .HasMaxLength(50);

            modelBuilder.Entity<Microservice>()
                .Property(m => m.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Microservice>()
                .Property(m => m.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configure BackupChains table
            modelBuilder.Entity<BackupChain>()
                .ToTable("backup_chains", "backup")
                .HasKey(bc => bc.Id);

            modelBuilder.Entity<BackupChain>()
                .Property(bc => bc.MicroserviceId)
                .IsRequired();

            modelBuilder.Entity<BackupChain>()
                .Property(bc => bc.MicroserviceName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<BackupChain>()
                .Property(bc => bc.FullBackupFile)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<BackupChain>()
                .Property(bc => bc.Timestamp)
                .IsRequired();

            modelBuilder.Entity<BackupChain>()
                .Property(bc => bc.Incrementals)
                .HasColumnType("text[]")
                .HasDefaultValueSql("'{}'::text[]");

            // Define relationship: BackupChain -> Microservice
            modelBuilder.Entity<BackupChain>()
                .HasOne<Microservice>()
                .WithMany()
                .HasForeignKey(bc => bc.MicroserviceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index for performance
            modelBuilder.Entity<BackupChain>()
                .HasIndex(bc => bc.MicroserviceId);

            modelBuilder.Entity<BackupChain>()
                .HasIndex(bc => bc.MicroserviceName);
        }
}