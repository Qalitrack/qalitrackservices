using ArchiveService.Core.DTOs;
using ArchiveService.Core.Entities;

namespace ArchiveService.Core.Interfaces
{
    public interface IArchivalEngine
    {
        Task<ArchiveResultDto> ArchiveTransactionsAsync(ArchiveRequestDto request, CancellationToken cancellationToken = default);
        Task<ArchiveResultDto> ArchiveWeightMeasurementsAsync(ArchiveRequestDto request, CancellationToken cancellationToken = default);
        Task<ArchiveResultDto> ArchiveComplianceRecordsAsync(ArchiveRequestDto request, CancellationToken cancellationToken = default);
        Task<RestoreResultDto> RestoreDataAsync(RestoreRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteArchivedDataAsync(string archiveId, CancellationToken cancellationToken = default);
        Task<ArchiveResultDto> GetArchiveStatusAsync(string archiveId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ArchiveResultDto>> GetArchiveHistoryAsync(string entityType, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
        Task<bool> ValidateArchiveIntegrityAsync(string archiveId, CancellationToken cancellationToken = default);
        Task<byte[]> ExportArchiveAsync(string archiveId, string format = "JSON", CancellationToken cancellationToken = default);
        Task<long> GetArchiveSizeAsync(string archiveId, CancellationToken cancellationToken = default);
    }
}