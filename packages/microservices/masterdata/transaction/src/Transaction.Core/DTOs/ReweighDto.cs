namespace Transaction.Core.DTOs;

public class ReweighRecordDto
{
    public int Id { get; set; }
    public int WeighbridgeTransactionId { get; set; }
    public int AttemptNumber { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = "Pending";
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

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ApproveReweighDto
{
    public string TicketID { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
    public string? Notes { get; set; }
}

public class RejectReweighDto
{
    public string TicketID { get; set; } = string.Empty;
    public string RejectionReason { get; set; } = string.Empty;
    public string? RejectedBy { get; set; }
    public string? Notes { get; set; }
}





