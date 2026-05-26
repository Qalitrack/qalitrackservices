using System;
using System.Collections.Generic;

namespace Masterdata.Core.DTOs.Weighbridge;

public class WeighbridgeDto
{
    public Guid Id { get; set; }
    public string Location { get; set; } = null!;
    public string? Description { get; set; }
    public string? Status { get; set; }
    public List<string> Scales { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
