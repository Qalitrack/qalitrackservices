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
            ?? throw new InvalidOperationException(
                "ConnectionStrings__DefaultConnection is not set");

        optionsBuilder.UseNpgsql(connectionString);
        
        return new MasterdataDbContext(optionsBuilder.Options);
    }
}
