namespace SupplierService.Core.Entities;

public class SupplierDocument : BaseEntity
{
    public string SupplierId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DocumentType DocumentType { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public string? FileUrl { get; set; }
    public long? FileSize { get; set; }
    public string? MimeType { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Version { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Active;
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual Supplier Supplier { get; set; } = null!;
}

public enum DocumentType
{
    Registration,
    TaxCertificate,
    Insurance,
    License,
    Contract,
    QualityCertificate,
    ComplianceCertificate,
    FinancialStatement,
    BankReference,
    Other
}

public enum DocumentStatus
{
    Active,
    Expired,
    Pending,
    Rejected,
    Archived
}