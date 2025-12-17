namespace Transaction.Core.Entities;

public class WeighbridgeTransaction : BaseEntity
{
    // Receipt and Identification
    public string ReceiptNo { get; set; } = string.Empty;
    public int ExpectedWeighings { get; set; } = 2; // Default to 2 weighings
    public int CompletedWeighings { get; set; } = 0;
    
    // Weight Information
    public decimal? NetWeight { get; set; }
    public DateTime? NetWeightCalculatedTimestamp { get; set; }
    
    // Vehicle Information
    public Guid? VehicleId { get; set; }
    public string NoPlate { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    
    // Commodity Information
    public Guid? CommodityId { get; set; }
    public string CommodityName { get; set; } = string.Empty;
    
    // Supplier Information
    public Guid? SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    
    // Customer Information
    public Guid? CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    
    // Transporter Information (Required)
    public Guid TransporterId { get; set; }
    public string TransporterName { get; set; } = string.Empty;
    
    // Origin and Destination
    public Guid? OriginId { get; set; }
    public string OriginName { get; set; } = string.Empty;
    public Guid? DestinationId { get; set; }
    public string DestinationName { get; set; } = string.Empty;
    
    // Weighbridge Information - First Weighing
    public Guid? WeighBridgeId { get; set; }
    public string WeighBridgeName { get; set; } = string.Empty;
    public string ScaleName { get; set; } = string.Empty;
    public Guid? OperatorId { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    
    // Image fields
    public string NPR { get; set; } = string.Empty;  // Image path or base64 string for NPR
    public string Image { get; set; } = string.Empty; // Image path or base64 string for general image
    
    // Weighbridge Information - Second Weighing
    public string WeighBridgeName2nd { get; set; } = string.Empty;
    public string ScaleName2nd { get; set; } = string.Empty;
    public Guid? OperatorId2nd { get; set; }
    public string OperatorName2nd { get; set; } = string.Empty;
    
    // Direction of the weighing operation (Inbound/Outbound)
    public WeighingDirection WeighMode { get; set; }
    
    // Transaction Status
    public WeighbridgeTransactionStatus Status { get; set; } = WeighbridgeTransactionStatus.Pending;
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedDate { get; set; }
    
    // Reweigh and Modifications
    public bool IsReweighRequested => Status == WeighbridgeTransactionStatus.ReweighRequested;
    public bool IsReweighInProgress => Status == WeighbridgeTransactionStatus.ReweighInProgress;
    public bool ReweighPermissionGranted { get; set; }
    public string? ReweighPermissionReason { get; set; }
    public string? ReweighReason { get; private set; }
    public DateTime? ReweighRequestDate { get; private set; }
    public string? ReweighRequestedBy { get; private set; }
    public int CurrentReweighAttempt { get; private set; } = 0;
    public int MaxReweighAttempts { get; set; } = 3; // Default to 3 attempts
    public string? ChangeDescription { get; set; }
    public DateTime? ChangeDate { get; set; }
    
    // Navigation property for reweigh records
    public virtual ICollection<ReweighRecord> ReweighRecords { get; set; } = new List<ReweighRecord>();
    
    // Navigation Properties
    public virtual ICollection<WeighingRecord> WeighingRecords { get; set; } = new List<WeighingRecord>();
    public virtual ICollection<TransactionAuditLog> AuditLogs { get; set; } = new List<TransactionAuditLog>();
    
    // Methods
    public bool CanBeModified()
    {
        return !IsCompleted;
    }
    
    public bool CanAddWeighing()
    {
        return !IsCompleted && CompletedWeighings < ExpectedWeighings;
    }
    
    public void CompleteTransaction(DateTime currentTime)
    {
        if (IsCompleted)
        {
            throw new InvalidOperationException("Transaction is already completed.");
        }

        // Ensure we have the expected number of weighings
        if (WeighingRecords == null || WeighingRecords.Count < ExpectedWeighings)
        {
            throw new InvalidOperationException(
                $"Cannot complete transaction. Expected {ExpectedWeighings} weighings, but only {WeighingRecords?.Count ?? 0} completed.");
        }

        // Update transaction status
        IsCompleted = true;
        Status = WeighbridgeTransactionStatus.Completed;
        CompletedDate = currentTime;
        UpdatedAt = currentTime;
        CompletedWeighings = WeighingRecords.Count;

        // Calculate net weight if not already set
        if (!NetWeight.HasValue && WeighingRecords.Count >= 2)
        {
            try
            {
                // Get first and last weighings
                var firstWeighing = WeighingRecords.OrderBy(w => w.WeighingSequence).First();
                var lastWeighing = WeighingRecords.OrderByDescending(w => w.WeighingSequence).First();
            
                // Calculate net weight (absolute difference between first and last weight)
                NetWeight = Math.Abs(firstWeighing.Weight - lastWeighing.Weight);
                NetWeightCalculatedTimestamp = currentTime;
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the transaction completion
                Console.WriteLine($"Error calculating net weight: {ex.Message}");
            }
        }
    }

    public void RequestReweigh(string reason, string requestedBy, DateTime currentTime)
    {
        if (!IsCompleted)
        {
            throw new InvalidOperationException("Cannot request reweigh on an incomplete transaction.");
        }

        if (IsReweighRequested || IsReweighInProgress)
        {
            throw new InvalidOperationException("Reweigh has already been requested or is in progress.");
        }

        Status = WeighbridgeTransactionStatus.ReweighRequested;
        ReweighReason = reason;
        ReweighRequestDate = currentTime;
        ReweighRequestedBy = requestedBy;
        UpdatedAt = currentTime;
    }

    public void StartReweigh(DateTime currentTime)
    {
        if (!IsReweighRequested)
        {
            throw new InvalidOperationException("Cannot start reweigh that hasn't been requested.");
        }

        Status = WeighbridgeTransactionStatus.ReweighInProgress;
        IsCompleted = false;
        CompletedDate = null;
        UpdatedAt = currentTime;
    }

    public void CompleteReweigh(DateTime currentTime)
    {
        if (!IsReweighInProgress)
        {
            throw new InvalidOperationException("Cannot complete reweigh that isn't in progress.");
        }

        CompleteTransaction(currentTime);
    }
    
    public void AddReweighWeight(decimal weight, string operatorName, DateTime currentTime, string? notes = null)
    {
        if (!IsReweighInProgress)
        {
            throw new InvalidOperationException("Cannot add weight - no reweigh in progress");
        }

        var currentReweigh = ReweighRecords.FirstOrDefault(r => r.AttemptNumber == CurrentReweighAttempt);
        if (currentReweigh == null)
        {
            throw new InvalidOperationException("No active reweigh record found for the current attempt");
        }

        if (!currentReweigh.Weight1.HasValue)
        {
            currentReweigh.Weight1 = weight;
            currentReweigh.Operator1 = operatorName;
            currentReweigh.Weight1Timestamp = currentTime;
        }
        else if (!currentReweigh.Weight2.HasValue)
        {
            currentReweigh.Weight2 = weight;
            currentReweigh.Operator2 = operatorName;
            currentReweigh.Weight2Timestamp = currentTime;
            currentReweigh.NetWeight = Math.Abs(currentReweigh.Weight1.Value - (decimal)currentReweigh.Weight2.Value);
        }
        else
        {
            throw new InvalidOperationException("Maximum weights (2) already recorded for this reweigh attempt");
        }

        if (!string.IsNullOrEmpty(notes))
        {
            currentReweigh.Notes = string.IsNullOrEmpty(currentReweigh.Notes)
                ? notes
                : $"{currentReweigh.Notes}\n{notes}";
        }

        UpdatedAt = currentTime;
    }
}


public class ReweighRecord
{
    public int Id { get; set; }
    public string WeighbridgeTransactionId { get; set; } = string.Empty;
    public int AttemptNumber { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Cancelled
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public string? PerformedBy { get; set; }
    
    // Weight measurements
    public decimal? Weight1 { get; set; }
    public string? Operator1 { get; set; }
    public DateTime? Weight1Timestamp { get; set; }
    
    public decimal? Weight2 { get; set; }
    public string? Operator2 { get; set; }
    public DateTime? Weight2Timestamp { get; set; }
    
    public decimal? NetWeight { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Soft delete
    public bool IsDeleted { get; set; }
    
    // Navigation property
    public virtual WeighbridgeTransaction? WeighbridgeTransaction { get; set; }
}

public enum WeighingDirection
{
    Inbound,
    Outbound,
    Unknown
}

public enum WeighbridgeTransactionStatus
{
    Pending,            // Initial state, awaiting weighings
    InProgress,         // At least one weighing done, more expected
    Completed,          // All weighings done, transaction immutable
    Cancelled,          // Transaction cancelled
    OnHold,             // Temporarily paused
    ReweighRequested,   // Requested for reweigh
    ReweighInProgress   // Reweigh in progress
}

// Value Object for weight measurement
public class WeightMeasurement
{
    public decimal Value { get; private set; }
    public string Unit { get; private set; } = "KG";
    public DateTime MeasuredAt { get; private set; }
    
    public WeightMeasurement(decimal value, DateTime measuredAt, string unit = "KG")
    {
        if (value < 0)
            throw new ArgumentException("Weight cannot be negative", nameof(value));
         
        Value = value;
        Unit = unit;
        MeasuredAt = measuredAt;
    }
}
