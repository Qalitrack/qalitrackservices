namespace ArchiveService.Core.DTOs
{
    public class ArchiveResultDto
    {
        public string ArchiveId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int RecordsArchived { get; set; }
        public long OriginalSize { get; set; }
        public long CompressedSize { get; set; }
        public decimal CompressionRatio { get; set; }
        public string StorageLocation { get; set; } = string.Empty;
        public string StorageTier { get; set; } = string.Empty;
        public DateTime ArchiveDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ChecksumValue { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string[] Tags { get; set; } = Array.Empty<string>();
        public Dictionary<string, object> AdditionalMetadata { get; set; } = new Dictionary<string, object>();
    }
}