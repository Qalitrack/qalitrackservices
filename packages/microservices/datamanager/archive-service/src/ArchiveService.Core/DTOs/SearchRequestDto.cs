namespace ArchiveService.Core.DTOs
{
    public class SearchRequestDto
    {
        public string SearchTerm { get; set; } = string.Empty;
        public string[] EntityTypes { get; set; } = Array.Empty<string>();
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string[] OrganizationIds { get; set; } = Array.Empty<string>();
        public string[] VehicleIds { get; set; } = Array.Empty<string>();
        public string[] DriverIds { get; set; } = Array.Empty<string>();
        public string[] WeighbridgeIds { get; set; } = Array.Empty<string>();
        public string[] Tags { get; set; } = Array.Empty<string>();
        public Dictionary<string, object> CustomFilters { get; set; } = new Dictionary<string, object>();
        public string SortBy { get; set; } = "ArchivedDate";
        public string SortOrder { get; set; } = "DESC";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public bool IncludeMetadata { get; set; } = true;
        public bool HighlightResults { get; set; } = true;
        public string SearchType { get; set; } = "FUZZY"; // EXACT, FUZZY, WILDCARD
    }
}