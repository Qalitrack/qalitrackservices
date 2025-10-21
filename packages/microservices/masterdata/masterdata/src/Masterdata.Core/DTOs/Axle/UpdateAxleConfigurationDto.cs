using System.ComponentModel.DataAnnotations;

namespace Masterdata.Core.DTOs.Axle;

public class UpdateAxleConfigurationDto
{
    [Required]
    public string Id { get; set; } = null!;

    [MaxLength(20)]
    public string? Code { get; set; }

    [MaxLength(200)]
    public string? Description { get; set; }

    public int? AxleCount { get; set; }
    
    public decimal? MaxLoadCapacity { get; set; }
    
    public bool? IsActive { get; set; }
}