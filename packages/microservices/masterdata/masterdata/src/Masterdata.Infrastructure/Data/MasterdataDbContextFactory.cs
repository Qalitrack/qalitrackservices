using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Masterdata.Infrastructure.Data;

public class MasterdataDbContextFactory : IDesignTimeDbContextFactory<MasterdataDbContext>
{
    public MasterdataDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MasterdataDbContext>();
        
        // Use PostgreSQL for migrations
        // This connection string is for design-time only (migrations)
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=masterdatadb;Username=masterdata;Password=masterdata123;Pooling=true;MinPoolSize=5;MaxPoolSize=100;IncludeErrorDetail=true;CommandTimeout=60";

        optionsBuilder.UseNpgsql(connectionString);
        
        return new MasterdataDbContext(optionsBuilder.Options);
    }
}
