using QaliTrack.DataManager.Core.Common;

namespace QaliTrack.DataManager.Core.Modules.Transactions.Entities;

public class WeighingTransaction : BaseEntity
{
    public string TransactionNumber { get; set; } = string.Empty;
    public Guid SiteId { get; set; }
    public Guid WeighbridgeId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid DriverId { get; set; }
    public Guid? CustomerOrderId { get; set; }
    public string TransactionType { get; set; } = string.Empty; // Outbound, Inbound, InterPlant
    public string Direction { get; set; } = string.Empty; // In, Out
    public string Status { get; set; } = "WeightEntry";
    public DateTime TransactionDate { get; set; }
    
    // Weight Information
    public decimal? GrossWeight { get; set; }
    public decimal? TareWeight { get; set; }
    public decimal? NetWeight { get; set; }
    public DateTime? EntryWeighingTime { get; set; }
    public DateTime? ExitWeighingTime { get; set; }
    
    // Container and Seal Information
    public string? SealNumber { get; set; }
    public string CurrentState { get; set; } = "TareWeighed";
    
    // Navigation properties
    public virtual ICollection<TransactionLine> TransactionLines { get; set; } = new List<TransactionLine>();
    public virtual ICollection<TransactionAudit> AuditTrail { get; set; } = new List<TransactionAudit>();
}

/// <summary>
/// Individual product lines within a weighing transaction
/// Links to ProductVariant from MasterData ProductCatalog
/// </summary>
public class TransactionLine : BaseEntity
{
    public Guid TransactionId { get; set; }
    public Guid ProductVariantId { get; set; }
    public decimal Quantity { get; set; }
    public decimal? ActualUnitWeight { get; set; }
    public string? BatchNumber { get; set; }
    public string? QualityGrade { get; set; }
    public decimal LineTotalWeight { get; set; }

    // Navigation properties
    public virtual WeighingTransaction Transaction { get; set; } = null!;
}

/// <summary>
/// Audit trail for transaction changes
/// </summary>
public class TransactionAudit : BaseEntity
{
    public Guid TransactionId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime PerformedAt { get; set; }
    public string? Comments { get; set; }
    
    // Navigation properties
    public virtual WeighingTransaction Transaction { get; set; } = null!;
}