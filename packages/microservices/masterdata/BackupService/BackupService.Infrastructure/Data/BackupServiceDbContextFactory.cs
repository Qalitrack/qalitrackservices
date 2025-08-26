using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BackupService.Infrastructure.Data;

public class BackupServiceDbContextFactory : IDesignTimeDbContextFactory<BackupServiceDbContext>
{
    public BackupServiceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BackupServiceDbContext>();
        optionsBuilder.UseNpgsql( "Host=localhost;Port=5532;Database=backupservicedb;Username=backupservice;Password=backupservice123");
        return new BackupServiceDbContext(optionsBuilder.Options);
    }
}