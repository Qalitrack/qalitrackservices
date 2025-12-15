namespace Transaction.Core.DTOs;


public class AddWeighingDto
{
    public string TransactionId { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public Guid WeighBridgeId { get; set; }
    public string WeighBridgeName { get; set; } = string.Empty;
    public string ScaleName { get; set; } = string.Empty;
    public Guid OperatorId { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public string? Notes { get; set; }
}