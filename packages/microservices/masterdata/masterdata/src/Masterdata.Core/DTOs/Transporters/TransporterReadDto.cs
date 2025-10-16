namespace Masterdata.Core.DTOs.Transporters;

public class TransporterReadDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? ContactInfo { get; set; }
    public string? Status { get; set; }
    public string? Logo { get; set; }
}
