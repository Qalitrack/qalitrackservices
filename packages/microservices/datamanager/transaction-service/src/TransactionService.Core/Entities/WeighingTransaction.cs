using Newtonsoft.Json;

namespace TransactionService.Core.Entities;

public class WeighingTransaction : BaseEntity
{
    public string TransactionNumber { get; set; } = string.Empty;
    public TransactionType TransactionType { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    
    // Master Data References
    public string VehicleId { get; set; } = string.Empty;
    public string DriverId { get; set; } = string.Empty;
    public string SupplierId { get; set; } = string.Empty;
    public string? CustomerId { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string RouteId { get; set; } = string.Empty;
    public string WeighbridgeId { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    
    // Weight Information
    public decimal? GrossWeight { get; set; }
    public decimal? TareWeight { get; set; }
    public decimal? NetWeight { get; set; }
    public DateTime? EntryWeighingTime { get; set; }
    public DateTime? ExitWeighingTime { get; set; }
    
    // Additional Information
    public string? DeliveryNoteNumber { get; set; }
    public string? PermitNumber { get; set; }
    public string? Remarks { get; set; }
    public string? MetadataJson { get; set; }
    
    // Lifecycle and State Management
    public string CurrentState { get; set; } = TransactionStates.Pending;
    public DateTime? StateLastChanged { get; set; }
    public string? StateChangedBy { get; set; }
    
    // Orchestration and Coordination
    public bool RequiresApproval { get; set; } = false;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int Priority { get; set; } = 1; // 1=Low, 2=Normal, 3=High, 4=Critical
    public string? ExternalReferenceId { get; set; }
    
    // Navigation Properties
    public virtual ICollection<TransactionWorkflow> WorkflowSteps { get; set; } = new List<TransactionWorkflow>();
    public virtual ICollection<TransactionCharge> Charges { get; set; } = new List<TransactionCharge>();
    public virtual ICollection<TransactionDocument> Documents { get; set; } = new List<TransactionDocument>();
    public virtual ICollection<TransactionState> StateHistory { get; set; } = new List<TransactionState>();
    public virtual ICollection<TransactionAudit> AuditTrail { get; set; } = new List<TransactionAudit>();
    
    // Helper property for metadata
    public Dictionary<string, object>? Metadata
    {
        get => string.IsNullOrEmpty(MetadataJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(MetadataJson);
        set => MetadataJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
}