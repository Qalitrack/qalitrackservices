namespace ArchiveService.Core.DTOs
{
    public class RestoreResultDto
    {
        public string RestoreId { get; set; } = string.Empty;
        public string ArchiveId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int RecordsRestored { get; set; }
        public int TotalRecords { get; set; }
        public int FailedRecords { get; set; }
        public decimal ProgressPercentage { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime RestoreDate { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TimeSpan? Duration { get; set; }
        public string RestoreLocation { get; set; } = string.Empty;
        public string RequestedBy { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string[] ErrorDetails { get; set; } = Array.Empty<string>();
        public Dictionary<string, object> RestoreOptions { get; set; } = new Dictionary<string, object>();
    }
}