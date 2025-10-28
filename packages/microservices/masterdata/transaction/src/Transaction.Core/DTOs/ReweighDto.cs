namespace Transaction.Core.DTOs;

public class StartReweighDto
{
    public string StartedBy { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class AddReweighWeightDto
{
    public string TransactionId { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class CompleteReweighDto
{
    public string TransactionId { get; set; } = string.Empty;
    public string CompletedBy { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class ReweighRecordDto
{
    public int Id { get; set; }
    public int AttemptNumber { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public string? PerformedBy { get; set; }
    public decimal? Weight1 { get; set; }
    public decimal? Weight2 { get; set; }
    public decimal? NetWeight { get; set; }
    public string WeighbridgeTransactionId { get; set; } = string.Empty;
}
