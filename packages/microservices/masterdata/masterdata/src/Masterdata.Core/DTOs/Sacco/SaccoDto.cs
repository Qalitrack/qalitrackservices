using System.Text.Json;

namespace Masterdata.Core.DTOs.Sacco;

public class SaccoDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public JsonDocument? ContactInfo { get; set; }
    public JsonDocument? OtherDetails { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
