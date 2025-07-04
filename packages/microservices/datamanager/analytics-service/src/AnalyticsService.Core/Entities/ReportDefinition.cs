using System.ComponentModel.DataAnnotations;

namespace AnalyticsService.Core.Entities;

public class ReportDefinition
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string ReportType { get; set; } = string.Empty; // Operational, Financial, Compliance, Custom
    
    [Required]
    [MaxLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    public List<string> MetricTypes { get; set; } = new();
    
    public Dictionary<string, object> Parameters { get; set; } = new();
    
    public Dictionary<string, object> FilterCriteria { get; set; } = new();
    
    [MaxLength(20)]
    public string DefaultTimeRange { get; set; } = "30d";
    
    [MaxLength(50)]
    public string OutputFormat { get; set; } = "PDF"; // PDF, Excel, CSV, HTML
    
    public bool IsScheduled { get; set; } = false;
    
    [MaxLength(100)]
    public string? ScheduleCron { get; set; } // Cron expression for scheduling
    
    public List<string> Recipients { get; set; } = new(); // Email addresses
    
    public bool IsActive { get; set; } = true;
    
    public DateTime LastGenerated { get; set; }
    
    public DateTime NextScheduledRun { get; set; }
    
    public Dictionary<string, object> TemplateConfiguration { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    [MaxLength(50)]
    public string CreatedBy { get; set; } = string.Empty;
}