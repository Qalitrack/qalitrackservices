namespace TransactionService.Core.Entities;

public class TransactionDocument : BaseEntity
{
    public string TransactionId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public string? ChecksumMd5 { get; set; }
    public string? ChecksumSha256 { get; set; }
    
    // Navigation Properties
    public virtual WeighingTransaction Transaction { get; set; } = null!;
}