namespace ArchiveService.Core.DTOs
{
    public class SearchResultDto
    {
        public long TotalHits { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public TimeSpan SearchTime { get; set; }
        public string SearchId { get; set; } = string.Empty;
        public ArchivedDocumentDto[] Documents { get; set; } = Array.Empty<ArchivedDocumentDto>();
        public Dictionary<string, long> Facets { get; set; } = new Dictionary<string, long>();
        public string[] Suggestions { get; set; } = Array.Empty<string>();
        public bool HasMore { get; set; }
        public string NextPageToken { get; set; } = string.Empty;
    }

    public class ArchivedDocumentDto
    {
        public string Id { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string ArchiveId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime ArchivedDate { get; set; }
        public DateTime OriginalDate { get; set; }
        public string[] Tags { get; set; } = Array.Empty<string>();
        public Dictionary<string, object> Content { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        public string[] Highlights { get; set; } = Array.Empty<string>();
        public decimal Score { get; set; }
        public string StorageLocation { get; set; } = string.Empty;
        public long Size { get; set; }
        public string ChecksumValue { get; set; } = string.Empty;
    }
}