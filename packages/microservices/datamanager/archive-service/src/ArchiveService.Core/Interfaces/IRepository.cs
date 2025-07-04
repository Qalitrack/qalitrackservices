using System.Linq.Expressions;

namespace ArchiveService.Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? filter = null, CancellationToken cancellationToken = default);
    }

    public interface IArchiveMetadataRepository : IRepository<ArchiveService.Core.Entities.ArchiveMetadata>
    {
        Task<IEnumerable<ArchiveService.Core.Entities.ArchiveMetadata>> GetByEntityTypeAsync(string entityType, CancellationToken cancellationToken = default);
        Task<ArchiveService.Core.Entities.ArchiveMetadata?> GetByArchiveIdAsync(string archiveId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ArchiveService.Core.Entities.ArchiveMetadata>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<IEnumerable<ArchiveService.Core.Entities.ArchiveMetadata>> GetByStorageTierAsync(string storageTier, CancellationToken cancellationToken = default);
        Task<long> GetTotalSizeByEntityTypeAsync(string entityType, CancellationToken cancellationToken = default);
    }

    public interface IRetentionPolicyRepository : IRepository<ArchiveService.Core.Entities.RetentionPolicy>
    {
        Task<IEnumerable<ArchiveService.Core.Entities.RetentionPolicy>> GetActivePoliciesAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<ArchiveService.Core.Entities.RetentionPolicy>> GetByEntityTypeAsync(string entityType, CancellationToken cancellationToken = default);
        Task<IEnumerable<ArchiveService.Core.Entities.RetentionPolicy>> GetDuePoliciesAsync(DateTime currentTime, CancellationToken cancellationToken = default);
    }

    public interface IDataMigrationRepository : IRepository<ArchiveService.Core.Entities.DataMigration>
    {
        Task<IEnumerable<ArchiveService.Core.Entities.DataMigration>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
        Task<IEnumerable<ArchiveService.Core.Entities.DataMigration>> GetRunningMigrationsAsync(CancellationToken cancellationToken = default);
        Task<ArchiveService.Core.Entities.DataMigration?> GetByMigrationIdAsync(string migrationId, CancellationToken cancellationToken = default);
    }
}