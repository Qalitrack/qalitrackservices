using Microsoft.EntityFrameworkCore;
using ArchiveService.Core.Entities;
using ArchiveService.Core.Interfaces;
using ArchiveService.Infrastructure.Data;

namespace ArchiveService.Infrastructure.Repositories
{
    public class ArchiveMetadataRepository : Repository<ArchiveMetadata>, IArchiveMetadataRepository
    {
        public ArchiveMetadataRepository(ArchiveDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ArchiveMetadata>> GetByEntityTypeAsync(string entityType, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(a => a.EntityType == entityType)
                .OrderByDescending(a => a.ArchiveDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<ArchiveMetadata?> GetByArchiveIdAsync(string archiveId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(a => a.RetentionPolicy)
                .Include(a => a.ArchiveStorage)
                .Include(a => a.ArchiveIndexes)
                .FirstOrDefaultAsync(a => a.ArchiveId == archiveId, cancellationToken);
        }

        public async Task<IEnumerable<ArchiveMetadata>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(a => a.ArchiveDate >= startDate && a.ArchiveDate <= endDate)
                .OrderByDescending(a => a.ArchiveDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ArchiveMetadata>> GetByStorageTierAsync(string storageTier, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(a => a.StorageTier == storageTier)
                .OrderByDescending(a => a.ArchiveDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<long> GetTotalSizeByEntityTypeAsync(string entityType, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(a => a.EntityType == entityType && a.Status == "ACTIVE")
                .SumAsync(a => a.CompressedSize, cancellationToken);
        }

        public override async Task<IEnumerable<ArchiveMetadata>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(a => a.RetentionPolicy)
                .Include(a => a.ArchiveStorage)
                .OrderByDescending(a => a.ArchiveDate)
                .ToListAsync(cancellationToken);
        }

        public override async Task<ArchiveMetadata?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(a => a.RetentionPolicy)
                .Include(a => a.ArchiveStorage)
                .Include(a => a.ArchiveIndexes)
                .Include(a => a.ArchivedTransactions)
                .Include(a => a.ArchivedWeightMeasurements)
                .Include(a => a.ArchivedComplianceRecords)
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }
    }
}