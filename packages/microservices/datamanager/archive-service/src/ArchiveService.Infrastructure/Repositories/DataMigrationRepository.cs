using Microsoft.EntityFrameworkCore;
using ArchiveService.Core.Entities;
using ArchiveService.Core.Interfaces;
using ArchiveService.Infrastructure.Data;

namespace ArchiveService.Infrastructure.Repositories
{
    public class DataMigrationRepository : Repository<DataMigration>, IDataMigrationRepository
    {
        public DataMigrationRepository(ArchiveDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<DataMigration>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(m => m.Status == status)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<DataMigration>> GetRunningMigrationsAsync(CancellationToken cancellationToken = default)
        {
            var runningStatuses = new[] { "PENDING", "RUNNING" };
            return await _dbSet
                .Where(m => runningStatuses.Contains(m.Status))
                .OrderBy(m => m.Priority)
                .ThenBy(m => m.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<DataMigration?> GetByMigrationIdAsync(string migrationId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(m => m.RetentionPolicy)
                .FirstOrDefaultAsync(m => m.MigrationId == migrationId, cancellationToken);
        }

        public override async Task<IEnumerable<DataMigration>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(m => m.RetentionPolicy)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public override async Task<DataMigration?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(m => m.RetentionPolicy)
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }
    }
}