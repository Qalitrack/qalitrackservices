using Microsoft.AspNetCore.Http;

namespace Transaction.Core.DTOs;

public class AddWeighingDto
{
    public int TicketID { get; set; }
    public decimal Weight { get; set; }
    public int? WeighBridgeId { get; set; }
    public string? WeighBridgeName { get; set; }
    public string? ScaleName { get; set; }
    public int? OperatorId { get; set; }
    public string? OperatorName { get; set; }
    public string? Notes { get; set; }
}