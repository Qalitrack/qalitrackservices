using System.ComponentModel.DataAnnotations;

namespace AnalyticsService.Core.Entities;

public class DashboardWidget
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string WidgetType { get; set; } = string.Empty; // Chart, KPI, Table, Gauge
    
    [Required]
    [MaxLength(50)]
    public string MetricType { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? DashboardId { get; set; }
    
    public int Position { get; set; }
    
    public int Width { get; set; } = 6; // Grid system (1-12)
    
    public int Height { get; set; } = 4; // Grid system
    
    public Dictionary<string, object> Configuration { get; set; } = new();
    
    public Dictionary<string, object> FilterCriteria { get; set; } = new();
    
    [MaxLength(20)]
    public string TimeRange { get; set; } = "24h"; // 1h, 24h, 7d, 30d, 90d
    
    public bool IsActive { get; set; } = true;
    
    public bool IsRealTime { get; set; } = false;
    
    public int RefreshInterval { get; set; } = 60; // seconds
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    [MaxLength(50)]
    public string CreatedBy { get; set; } = string.Empty;
}