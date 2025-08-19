namespace BackupService.Core.Entities;

public class BackupServiceResponseRecord
{ 
    public Guid Id { get; set; }
    public string CommandId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorDetails { get; set; }
    public DateTime ProcessedAt { get; set; }
}