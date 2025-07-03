namespace ArchiveService.Core.DTOs
{
    public class ArchiveRequestDto
    {
        public string EntityType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string[] OrganizationIds { get; set; } = Array.Empty<string>();
        public string[] VehicleIds { get; set; } = Array.Empty<string>();
        public string[] DriverIds { get; set; } = Array.Empty<string>();
        public string[] WeighbridgeIds { get; set; } = Array.Empty<string>();
        public Dictionary<string, object> CustomFilters { get; set; } = new Dictionary<string, object>();
        public string StorageTier { get; set; } = "HOT";
        public string CompressionType { get; set; } = "GZIP";
        public string[] Tags { get; set; } = Array.Empty<string>();
        public string Description { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public bool DeleteAfterArchive { get; set; } = true;
        public bool CreateIndex { get; set; } = true;
        public string Priority { get; set; } = "MEDIUM";
        public string RequestedBy { get; set; } = string.Empty;
        public Dictionary<string, object> AdditionalMetadata { get; set; } = new Dictionary<string, object>();
    }

    public class RestoreRequestDto
    {
        public string ArchiveId { get; set; } = string.Empty;
        public string RestoreLocation { get; set; } = string.Empty;
        public bool RestoreToOriginalLocation { get; set; } = true;
        public string[] RecordIds { get; set; } = Array.Empty<string>();
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Dictionary<string, object> CustomFilters { get; set; } = new Dictionary<string, object>();
        public bool OverwriteExisting { get; set; } = false;
        public string Priority { get; set; } = "MEDIUM";
        public string RequestedBy { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public Dictionary<string, object> RestoreOptions { get; set; } = new Dictionary<string, object>();
    }
}