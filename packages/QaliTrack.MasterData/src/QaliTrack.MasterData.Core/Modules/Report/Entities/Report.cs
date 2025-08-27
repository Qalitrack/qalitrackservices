using QaliTrack.MasterData.Core.Common;

namespace QaliTrack.MasterData.Core.Modules.Report.Entities;

public class Report : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public string DataSource { get; set; } = string.Empty;
    public string Parameters { get; set; } = string.Empty;
    public string OutputFormat { get; set; } = "PDF";
    public bool IsPublic { get; set; } = false;
    public bool RequiresApproval { get; set; } = false;
    public int ExecutionTimeoutMinutes { get; set; } = 30;
    public string Tags { get; set; } = string.Empty;
    public Guid OrganizationId { get; set; }

    public virtual ICollection<ReportTemplate> Templates { get; set; } = new List<ReportTemplate>();
    public virtual ICollection<ReportSchedule> Schedules { get; set; } = new List<ReportSchedule>();
    public virtual ICollection<ReportExecution> Executions { get; set; } = new List<ReportExecution>();
    public virtual ICollection<ReportPermission> Permissions { get; set; } = new List<ReportPermission>();
}

public class ReportTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string TemplateContent { get; set; } = string.Empty;
    public string TemplateEngine { get; set; } = "Razor";
    public string OutputFormat { get; set; } = "PDF";
    public string HeaderContent { get; set; } = string.Empty;
    public string FooterContent { get; set; } = string.Empty;
    public string StyleSheet { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = false;
    public string Version { get; set; } = "1.0";
    public Guid ReportId { get; set; }

    public virtual Report Report { get; set; } = null!;
}

public class ReportSchedule : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? NextRunTime { get; set; }
    public DateTime? LastRunTime { get; set; }
    public string Status { get; set; } = "Active";
    public bool IsEnabled { get; set; } = true;
    public string Parameters { get; set; } = string.Empty;
    public string Recipients { get; set; } = string.Empty;
    public string DeliveryMethod { get; set; } = "Email";
    public int RetryAttempts { get; set; } = 3;
    public int MaxRetries { get; set; } = 5;
    public Guid ReportId { get; set; }

    public virtual Report Report { get; set; } = null!;
    public virtual ICollection<ReportExecution> Executions { get; set; } = new List<ReportExecution>();
}

public class ReportExecution : BaseEntity
{
    public string ExecutionId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Status { get; set; } = "Pending";
    public string Parameters { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public int RecordCount { get; set; } = 0;
    public long FileSizeBytes { get; set; } = 0;
    public int ExecutionTimeSeconds { get; set; } = 0;
    public string ExecutedBy { get; set; } = string.Empty;
    public bool IsScheduled { get; set; } = false;
    public Guid ReportId { get; set; }
    public Guid? ScheduleId { get; set; }

    public virtual Report Report { get; set; } = null!;
    public virtual ReportSchedule? Schedule { get; set; }
}

public class ReportPermission : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string Permission { get; set; } = string.Empty;
    public bool CanView { get; set; } = true;
    public bool CanExecute { get; set; } = false;
    public bool CanSchedule { get; set; } = false;
    public bool CanEdit { get; set; } = false;
    public bool CanDelete { get; set; } = false;
    public DateTime? ExpiryDate { get; set; }
    public string GrantedBy { get; set; } = string.Empty;
    public Guid ReportId { get; set; }

    public virtual Report Report { get; set; } = null!;
}