using Microsoft.EntityFrameworkCore;
using ArchiveService.Core.Entities;
using ArchiveService.Core.Interfaces;
using ArchiveService.Infrastructure.Data;

namespace ArchiveService.Infrastructure.Repositories
{
    public class RetentionPolicyRepository : Repository<RetentionPolicy>, IRetentionPolicyRepository
    {
        public RetentionPolicyRepository(ArchiveDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<RetentionPolicy>> GetActivePoliciesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.IsActive)
                .OrderBy(p => p.Priority)
                .ThenBy(p => p.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<RetentionPolicy>> GetByEntityTypeAsync(string entityType, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.EntityType == entityType)
                .OrderBy(p => p.Priority)
                .ThenBy(p => p.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<RetentionPolicy>> GetDuePoliciesAsync(DateTime currentTime, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.IsActive && 
                           p.IsAutomatic && 
                           p.NextExecution.HasValue && 
                           p.NextExecution.Value <= currentTime)
                .OrderBy(p => p.Priority)
                .ThenBy(p => p.NextExecution)
                .ToListAsync(cancellationToken);
        }

        public override async Task<IEnumerable<RetentionPolicy>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.ArchiveMetadatas)
                .Include(p => p.DataMigrations)
                .OrderBy(p => p.Name)
                .ToListAsync(cancellationToken);
        }

        public override async Task<RetentionPolicy?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.ArchiveMetadatas)
                .Include(p => p.DataMigrations)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }
    }
}