namespace BackupService.Core.Entities;

public class BackupOperationRecord
{
    public string CommandId { get; set; } = string.Empty;
    public string CommandType { get; set; } = string.Empty;
    public DateTime InitiatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsSuccessful { get; set; }
    public string Status { get; set; } = "Initiated";
    public string? ResultMessage { get; set; }
    public bool IsCritical { get; set; }
    public string? ChainId { get; set; }
    public List<BackupServiceResponseRecord>? ServiceResponses { get; set; }

}