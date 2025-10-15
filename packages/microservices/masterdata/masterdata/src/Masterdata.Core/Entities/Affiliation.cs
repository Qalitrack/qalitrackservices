using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masterdata.Core.Entities;

public class Affiliation : BaseEntity
{
    public string? SaccoId { get; set; }

    [ForeignKey(nameof(SaccoId))]
    public virtual Sacco? Sacco { get; set; }

    public string? OrganisationId { get; set; }

    [ForeignKey(nameof(OrganisationId))]
    public virtual Organisation? Organisation { get; set; }

    public string? Type { get; set; }

    public string? Details { get; set; }
}
