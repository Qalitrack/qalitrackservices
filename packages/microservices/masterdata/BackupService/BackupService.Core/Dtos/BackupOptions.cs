namespace BackupService.Core.Dtos;

public class BackupOptions
{
    public string? Path { get; init; }
    public int? RetentionDays { get; init; } = 30;
}