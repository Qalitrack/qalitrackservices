using Microsoft.AspNetCore.Mvc;
using ArchiveService.Core.DTOs;
using ArchiveService.Core.Interfaces;

namespace ArchiveService.Api.Controllers
{
    /// <summary>
    /// Archive Search Controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Archive Search")]
    public class SearchController : BaseController
    {
        private readonly IArchiveSearchService _searchService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(IArchiveSearchService searchService, ILogger<SearchController> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        /// <summary>
        /// Search archived data across all entity types
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Search([FromBody] SearchRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Searching archived data with term: {SearchTerm}", request.SearchTerm);
                
                var result = await _searchService.SearchAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequestWithMessage(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching archived data");
                return HandleException(ex, "Failed to search archived data");
            }
        }

        /// <summary>
        /// Search archived transactions
        /// </summary>
        [HttpPost("transactions")]
        public async Task<IActionResult> SearchTransactions([FromBody] SearchRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _searchService.SearchTransactionsAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching archived transactions");
                return HandleException(ex, "Failed to search archived transactions");
            }
        }

        /// <summary>
        /// Search archived weight measurements
        /// </summary>
        [HttpPost("measurements")]
        public async Task<IActionResult> SearchWeightMeasurements([FromBody] SearchRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _searchService.SearchWeightMeasurementsAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching archived weight measurements");
                return HandleException(ex, "Failed to search archived weight measurements");
            }
        }

        /// <summary>
        /// Search archived compliance records
        /// </summary>
        [HttpPost("compliance")]
        public async Task<IActionResult> SearchComplianceRecords([FromBody] SearchRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _searchService.SearchComplianceRecordsAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching archived compliance records");
                return HandleException(ex, "Failed to search archived compliance records");
            }
        }

        /// <summary>
        /// Get a specific archived document by ID
        /// </summary>
        [HttpGet("document/{documentId}")]
        public async Task<IActionResult> GetDocument(string documentId, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _searchService.GetDocumentAsync(documentId, cancellationToken);
                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document {DocumentId}", documentId);
                return HandleException(ex, "Failed to get document");
            }
        }

        /// <summary>
        /// Get search facets for filtering
        /// </summary>
        [HttpPost("facets")]
        public async Task<IActionResult> GetSearchFacets([FromBody] SearchRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _searchService.GetSearchFacetsAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search facets");
                return HandleException(ex, "Failed to get search facets");
            }
        }

        /// <summary>
        /// Get search suggestions for auto-complete
        /// </summary>
        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSearchSuggestions(
            [FromQuery] string query,
            [FromQuery] string entityType = "",
            [FromQuery] int maxSuggestions = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequestWithMessage("Query parameter is required");
                }

                var result = await _searchService.GetSearchSuggestionsAsync(query, entityType, maxSuggestions, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search suggestions for query {Query}", query);
                return HandleException(ex, "Failed to get search suggestions");
            }
        }

        /// <summary>
        /// Create search index for an archive
        /// </summary>
        [HttpPost("index/{archiveId}")]
        public async Task<IActionResult> CreateIndex(string archiveId, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _searchService.IndexArchiveAsync(archiveId, cancellationToken);
                
                if (result)
                {
                    return Ok(new { message = "Search index created successfully", archiveId = archiveId });
                }
                
                return BadRequestWithMessage("Failed to create search index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating search index for archive {ArchiveId}", archiveId);
                return HandleException(ex, "Failed to create search index");
            }
        }

        /// <summary>
        /// Rebuild search index for an archive
        /// </summary>
        [HttpPost("reindex/{archiveId}")]
        public async Task<IActionResult> ReindexArchive(string archiveId, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _searchService.ReindexArchiveAsync(archiveId, cancellationToken);
                
                if (result)
                {
                    return Ok(new { message = "Search index rebuilt successfully", archiveId = archiveId });
                }
                
                return BadRequestWithMessage("Failed to rebuild search index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rebuilding search index for archive {ArchiveId}", archiveId);
                return HandleException(ex, "Failed to rebuild search index");
            }
        }

        /// <summary>
        /// Delete search index for an archive
        /// </summary>
        [HttpDelete("index/{archiveId}")]
        public async Task<IActionResult> DeleteIndex(string archiveId, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _searchService.DeleteIndexAsync(archiveId, cancellationToken);
                
                if (result)
                {
                    return Ok(new { message = "Search index deleted successfully", archiveId = archiveId });
                }
                
                return NotFound(new { error = "Search index not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting search index for archive {ArchiveId}", archiveId);
                return HandleException(ex, "Failed to delete search index");
            }
        }

        /// <summary>
        /// Check search index health
        /// </summary>
        [HttpGet("index/health/{archiveId}")]
        public async Task<IActionResult> CheckIndexHealth(string archiveId, CancellationToken cancellationToken = default)
        {
            try
            {
                var isHealthy = await _searchService.IsIndexHealthyAsync(archiveId, cancellationToken);
                
                return Ok(new { 
                    archiveId = archiveId,
                    isHealthy = isHealthy,
                    status = isHealthy ? "HEALTHY" : "UNHEALTHY"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking index health for archive {ArchiveId}", archiveId);
                return HandleException(ex, "Failed to check index health");
            }
        }

        /// <summary>
        /// Get search index statistics
        /// </summary>
        [HttpGet("index/stats/{archiveId}")]
        public async Task<IActionResult> GetIndexStatistics(string archiveId, CancellationToken cancellationToken = default)
        {
            try
            {
                var stats = await _searchService.GetIndexStatisticsAsync(archiveId, cancellationToken);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting index statistics for archive {ArchiveId}", archiveId);
                return HandleException(ex, "Failed to get index statistics");
            }
        }
    }
}