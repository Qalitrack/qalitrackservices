namespace Masterdata.Core.DTOs.Sacco;

public class SaccoDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? RegistrationNumber { get; set; }
    public string? OtherDetails { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
