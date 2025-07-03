using ArchiveService.Core.DTOs;

namespace ArchiveService.Core.Interfaces
{
    public interface IArchiveSearchService
    {
        Task<SearchResultDto> SearchAsync(SearchRequestDto request, CancellationToken cancellationToken = default);
        Task<SearchResultDto> SearchTransactionsAsync(SearchRequestDto request, CancellationToken cancellationToken = default);
        Task<SearchResultDto> SearchWeightMeasurementsAsync(SearchRequestDto request, CancellationToken cancellationToken = default);
        Task<SearchResultDto> SearchComplianceRecordsAsync(SearchRequestDto request, CancellationToken cancellationToken = default);
        Task<ArchivedDocumentDto?> GetDocumentAsync(string documentId, CancellationToken cancellationToken = default);
        Task<bool> IndexArchiveAsync(string archiveId, CancellationToken cancellationToken = default);
        Task<bool> ReindexArchiveAsync(string archiveId, CancellationToken cancellationToken = default);
        Task<bool> DeleteIndexAsync(string archiveId, CancellationToken cancellationToken = default);
        Task<Dictionary<string, long>> GetSearchFacetsAsync(SearchRequestDto request, CancellationToken cancellationToken = default);
        Task<string[]> GetSearchSuggestionsAsync(string query, string entityType, int maxSuggestions = 10, CancellationToken cancellationToken = default);
        Task<bool> IsIndexHealthyAsync(string archiveId, CancellationToken cancellationToken = default);
        Task<Dictionary<string, object>> GetIndexStatisticsAsync(string archiveId, CancellationToken cancellationToken = default);
    }
}