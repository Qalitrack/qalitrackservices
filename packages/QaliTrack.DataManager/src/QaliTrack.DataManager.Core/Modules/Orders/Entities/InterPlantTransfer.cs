using QaliTrack.DataManager.Core.Common;

namespace QaliTrack.DataManager.Core.Modules.Orders.Entities;

/// <summary>
/// Inter-plant transfers for cement operations (e.g., clinker to grinding stations)
/// </summary>
public class InterPlantTransfer : BaseEntity
{
    public string TransferNumber { get; set; } = string.Empty;
    public Guid FromSiteId { get; set; } // Source site
    public Guid ToSiteId { get; set; } // Destination site
    public DateTime TransferDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Approved, InProgress, Completed, Cancelled
    public string TransferType { get; set; } = string.Empty; // Regular, Emergency, Scheduled
    public string? TransferReason { get; set; }
    public string? SpecialInstructions { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }

    // Navigation properties
    public virtual ICollection<TransferLine> TransferLines { get; set; } = new List<TransferLine>();
}

/// <summary>
/// Individual line items within inter-plant transfers
/// </summary>
public class TransferLine : BaseEntity
{
    public Guid InterPlantTransferId { get; set; }
    public Guid ProductVariantId { get; set; } // Reference to ProductVariant in MasterData
    public decimal QuantityRequested { get; set; }
    public decimal QuantityTransferred { get; set; } = 0;
    public string LineStatus { get; set; } = "Pending"; // Pending, InProgress, Transferred, Cancelled
    public string? SpecialInstructions { get; set; }

    // Navigation properties
    public virtual InterPlantTransfer InterPlantTransfer { get; set; } = null!;
}