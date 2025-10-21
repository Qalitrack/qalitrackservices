using System.Text.Json.Serialization;

namespace Masterdata.Core.DTOs.Axle;

public class AxleConfigurationDto
{
    public string Id { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public int? AxleCount { get; set; }
    public decimal? MaxLoadCapacity { get; set; }
    public bool IsActive { get; set; }
}
