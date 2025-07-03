namespace CustomerService.Core.Entities;

public class CustomerDocument : BaseEntity
{
    public string CustomerId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string? OriginalFileName { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? FilePath { get; set; }
    public string? FileUrl { get; set; }
    public DocumentType DocumentType { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsConfidential { get; set; }
    public string? UploadedBy { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Active;

    public virtual Customer Customer { get; set; } = null!;
}

public enum DocumentType
{
    Contract,
    Invoice,
    Receipt,
    Certificate,
    License,
    Insurance,
    TaxDocument,
    ComplianceDoc,
    Identification,
    Other
}

public enum DocumentStatus
{
    Active,
    Archived,
    Expired,
    Deleted
}