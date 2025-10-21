using System;

namespace Masterdata.Core.DTOs.Route;

public class RouteDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string StartPoint { get; set; } = null!;
    public string EndPoint { get; set; } = null!;
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
