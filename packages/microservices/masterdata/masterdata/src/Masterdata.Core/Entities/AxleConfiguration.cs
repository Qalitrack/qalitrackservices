using System.ComponentModel.DataAnnotations;

namespace Masterdata.Core.Entities;

public class AxleConfiguration : BaseEntity
{
    [Required]
    public string Configuration { get; set; } = null!;

    public string? Description { get; set; }
}
