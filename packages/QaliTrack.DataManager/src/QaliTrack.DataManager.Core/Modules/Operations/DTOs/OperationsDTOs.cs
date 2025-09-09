namespace QaliTrack.DataManager.Core.Modules.Operations.DTOs;

// Operational Alert DTOs
public class OperationalAlertDto
{
    public Guid Id { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public DateTime AlertTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? AssignedTo { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    public string? AlertSource { get; set; }
    public string? AdditionalData { get; set; }
    public int EscalationLevel { get; set; }
    public DateTime? NextEscalationAt { get; set; }
    public Guid OrganizationId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateOperationalAlertDto
{
    public string AlertType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public DateTime AlertTime { get; set; }
    public string Status { get; set; } = "Open";
    public Guid? AssignedTo { get; set; }
    public string? AlertSource { get; set; }
    public string? AdditionalData { get; set; }
    public int EscalationLevel { get; set; } = 0;
    public DateTime? NextEscalationAt { get; set; }
    public Guid OrganizationId { get; set; }
}

public class UpdateOperationalAlertDto
{
    public string AlertType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? AssignedTo { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    public string? AdditionalData { get; set; }
    public int EscalationLevel { get; set; }
    public DateTime? NextEscalationAt { get; set; }
}