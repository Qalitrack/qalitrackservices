namespace QaliTrack.MasterData.Core.Modules.Report.DTOs;

public record CreateReportDto(
    string Name,
    string Code,
    string ReportType,
    string Category,
    string DataSource,
    Guid OrganizationId,
    string? Description = null,
    string Status = "Active",
    string Parameters = "",
    string OutputFormat = "PDF",
    bool IsPublic = false,
    bool RequiresApproval = false,
    int ExecutionTimeoutMinutes = 30,
    string Tags = ""
);

public record UpdateReportDto(
    string Name,
    string ReportType,
    string Category,
    string DataSource,
    string Status,
    string? Description = null,
    string Parameters = "",
    string OutputFormat = "PDF",
    bool IsPublic = false,
    bool RequiresApproval = false,
    int ExecutionTimeoutMinutes = 30,
    string Tags = ""
);

public record ReportSummaryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string ReportType { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string OutputFormat { get; init; } = string.Empty;
    public bool IsPublic { get; init; }
    public bool RequiresApproval { get; init; }
    public int TemplateCount { get; init; }
    public int ScheduleCount { get; init; }
    public int ExecutionCount { get; init; }
    public DateTime? LastExecutionTime { get; init; }
    public string LastExecutionStatus { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public record ReportDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ReportType { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string DataSource { get; init; } = string.Empty;
    public string Parameters { get; init; } = string.Empty;
    public string OutputFormat { get; init; } = string.Empty;
    public bool IsPublic { get; init; }
    public bool RequiresApproval { get; init; }
    public int ExecutionTimeoutMinutes { get; init; }
    public string Tags { get; init; } = string.Empty;
    public Guid OrganizationId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public bool HasTemplates { get; init; }
    public bool HasSchedules { get; init; }
    public bool HasExecutions { get; init; }
    public bool HasPermissions { get; init; }
}

// Report Template DTOs
public record CreateReportTemplateDto(
    string Name,
    string TemplateContent,
    string TemplateEngine = "Razor",
    string OutputFormat = "PDF",
    string HeaderContent = "",
    string FooterContent = "",
    string StyleSheet = "",
    bool IsDefault = false,
    string Version = "1.0"
);

public record UpdateReportTemplateDto(
    string Name,
    string TemplateContent,
    string TemplateEngine,
    string OutputFormat,
    string HeaderContent,
    string FooterContent,
    string StyleSheet,
    bool IsDefault,
    string Version
);

public record ReportTemplateDto
{
    public Guid Id { get; init; }
    public Guid ReportId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string TemplateContent { get; init; } = string.Empty;
    public string TemplateEngine { get; init; } = string.Empty;
    public string OutputFormat { get; init; } = string.Empty;
    public string HeaderContent { get; init; } = string.Empty;
    public string FooterContent { get; init; } = string.Empty;
    public string StyleSheet { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
    public string Version { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

// Report Schedule DTOs
public record CreateReportScheduleDto(
    string Name,
    string CronExpression,
    string Frequency,
    string Recipients,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string Status = "Active",
    bool IsEnabled = true,
    string Parameters = "",
    string DeliveryMethod = "Email",
    int RetryAttempts = 3,
    int MaxRetries = 5
);

public record UpdateReportScheduleDto(
    string Name,
    string CronExpression,
    string Frequency,
    string Recipients,
    DateTime? StartDate,
    DateTime? EndDate,
    string Status,
    bool IsEnabled,
    string Parameters,
    string DeliveryMethod,
    int RetryAttempts,
    int MaxRetries
);

public record ReportScheduleDto
{
    public Guid Id { get; init; }
    public Guid ReportId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string CronExpression { get; init; } = string.Empty;
    public string Frequency { get; init; } = string.Empty;
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public DateTime? NextRunTime { get; init; }
    public DateTime? LastRunTime { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public string Parameters { get; init; } = string.Empty;
    public string Recipients { get; init; } = string.Empty;
    public string DeliveryMethod { get; init; } = string.Empty;
    public int RetryAttempts { get; init; }
    public int MaxRetries { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public bool IsActive { get; init; }
    public bool IsOverdue { get; init; }
}

// Report Execution DTOs
public record CreateReportExecutionDto(
    string ExecutionId,
    string Parameters = "",
    string ExecutedBy = "System",
    bool IsScheduled = false,
    Guid? ScheduleId = null
);

public record ReportExecutionDto
{
    public Guid Id { get; init; }
    public Guid ReportId { get; init; }
    public string ExecutionId { get; init; } = string.Empty;
    public DateTime StartTime { get; init; }
    public DateTime? EndTime { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Parameters { get; init; } = string.Empty;
    public string OutputPath { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
    public int RecordCount { get; init; }
    public long FileSizeBytes { get; init; }
    public int ExecutionTimeSeconds { get; init; }
    public string ExecutedBy { get; init; } = string.Empty;
    public bool IsScheduled { get; init; }
    public Guid? ScheduleId { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsCompleted { get; init; }
    public bool HasError { get; init; }
    public string FormattedFileSize { get; init; } = string.Empty;
}

// Report Permission DTOs
public record CreateReportPermissionDto(
    string UserId,
    string UserEmail,
    string Permission,
    bool CanView = true,
    bool CanExecute = false,
    bool CanSchedule = false,
    bool CanEdit = false,
    bool CanDelete = false,
    DateTime? ExpiryDate = null,
    string GrantedBy = "System"
);

public record UpdateReportPermissionDto(
    string Permission,
    bool CanView,
    bool CanExecute,
    bool CanSchedule,
    bool CanEdit,
    bool CanDelete,
    DateTime? ExpiryDate,
    string GrantedBy
);

public record ReportPermissionDto
{
    public Guid Id { get; init; }
    public Guid ReportId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string UserEmail { get; init; } = string.Empty;
    public string Permission { get; init; } = string.Empty;
    public bool CanView { get; init; }
    public bool CanExecute { get; init; }
    public bool CanSchedule { get; init; }
    public bool CanEdit { get; init; }
    public bool CanDelete { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public string GrantedBy { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public bool IsExpired { get; init; }
}

// Report Execution Request
public record ReportExecutionRequestDto(
    string? Parameters = null,
    string? ExecutedBy = null
);