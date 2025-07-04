namespace ArchiveService.Core.DTOs
{
    public class RetentionPolicyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int RetentionDays { get; set; }
        public int ArchiveAfterDays { get; set; }
        public int DeleteAfterDays { get; set; }
        public string StorageTier { get; set; } = string.Empty;
        public string CompressionType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsAutomatic { get; set; }
        public string ExecutionSchedule { get; set; } = string.Empty;
        public DateTime? LastExecuted { get; set; }
        public DateTime? NextExecution { get; set; }
        public Dictionary<string, object> FilterCriteria { get; set; } = new Dictionary<string, object>();
        public string[] OrganizationIds { get; set; } = Array.Empty<string>();
        public string Priority { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Notes { get; set; } = string.Empty;
        public int ArchiveCount { get; set; }
        public long TotalArchivedSize { get; set; }
    }

    public class CreateRetentionPolicyDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int RetentionDays { get; set; }
        public int ArchiveAfterDays { get; set; }
        public int DeleteAfterDays { get; set; }
        public string StorageTier { get; set; } = "HOT";
        public string CompressionType { get; set; } = "GZIP";
        public bool IsActive { get; set; } = true;
        public bool IsAutomatic { get; set; } = true;
        public string ExecutionSchedule { get; set; } = "DAILY";
        public Dictionary<string, object> FilterCriteria { get; set; } = new Dictionary<string, object>();
        public string[] OrganizationIds { get; set; } = Array.Empty<string>();
        public string Priority { get; set; } = "MEDIUM";
        public string Notes { get; set; } = string.Empty;
    }

    public class UpdateRetentionPolicyDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? RetentionDays { get; set; }
        public int? ArchiveAfterDays { get; set; }
        public int? DeleteAfterDays { get; set; }
        public string? StorageTier { get; set; }
        public string? CompressionType { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsAutomatic { get; set; }
        public string? ExecutionSchedule { get; set; }
        public Dictionary<string, object>? FilterCriteria { get; set; }
        public string[]? OrganizationIds { get; set; }
        public string? Priority { get; set; }
        public string? Notes { get; set; }
    }
}