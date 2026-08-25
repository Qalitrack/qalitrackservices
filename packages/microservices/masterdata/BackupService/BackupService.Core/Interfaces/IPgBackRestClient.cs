using BackupService.Core.Dtos;
using BackupService.Core.Enums;

namespace BackupService.Core.Interfaces;

/// <summary>
/// Drives pgBackRest (full + incremental physical backups) against the shared
/// Postgres container. Unlike pg_dump/pg_restore, these operations need filesystem
/// access to PGDATA, so they run via `docker exec`/`docker run` against
/// postgres-prod rather than over a network connection.
/// </summary>
public interface IPgBackRestClient
{
    Task<PgBackRestBackupOutcome> BackupAsync(BackupType type, CancellationToken ct = default);
    Task<PgBackRestInfo> GetInfoAsync(CancellationToken ct = default);

    /// <summary>
    /// Stops postgres-prod, restores into its data volume via a one-off container, then
    /// starts postgres-prod back up. Restores everything the stanza covers at once —
    /// there is no per-schema/per-microservice restore with a physical backup.
    /// </summary>
    /// <param name="backupLabel">
    /// A specific pgBackRest backup label (full or incremental) to restore up to — passed
    /// as pgbackrest's --set. Null/omitted restores the latest backup in the chain.
    /// </param>
    Task RestoreAsync(string? backupLabel = null, CancellationToken ct = default);
}
