namespace Transaction.Core.DTOs;

public class WeighingRecordDto
{
    public string Id { get; set; } = string.Empty;
    public string WeighbridgeTransactionId { get; set; } = string.Empty;
    public int WeighingSequence { get; set; }
    public decimal Weight { get; set; }
    public DateTime WeighingDate { get; set; }
    public int? WeighBridgeId { get; set; }
    public string WeighBridgeName { get; set; } = string.Empty;
    public string ScaleName { get; set; } = string.Empty;
    public int? OperatorId { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
