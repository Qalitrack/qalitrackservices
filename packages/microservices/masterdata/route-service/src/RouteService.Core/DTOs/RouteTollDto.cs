using RouteService.Core.Entities;

namespace RouteService.Core.DTOs;

public class RouteTollDto
{
    public string Id { get; set; } = string.Empty;
    public string RouteId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public TollType Type { get; set; }
    public decimal BaseCost { get; set; }
    public decimal? CostPerAxle { get; set; }
    public decimal? CostPerWeight { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentMethod AcceptedPayments { get; set; }
    public bool IsElectronicTollCollection { get; set; }
    public string? ETCProvider { get; set; }
    public TimeOnly? OperatingHoursStart { get; set; }
    public TimeOnly? OperatingHoursEnd { get; set; }
    public bool Is24Hours { get; set; }
    public string? SpecialConditions { get; set; }
    public double? DistanceFromRouteStart { get; set; }
    public bool HasAlternativeRoute { get; set; }
    public string? AlternativeRouteDescription { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateRouteTollRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public TollType Type { get; set; }
    public decimal BaseCost { get; set; }
    public decimal? CostPerAxle { get; set; }
    public decimal? CostPerWeight { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentMethod AcceptedPayments { get; set; } = PaymentMethod.Cash;
    public bool IsElectronicTollCollection { get; set; } = false;
    public string? ETCProvider { get; set; }
    public TimeOnly? OperatingHoursStart { get; set; }
    public TimeOnly? OperatingHoursEnd { get; set; }
    public bool Is24Hours { get; set; } = true;
    public string? SpecialConditions { get; set; }
    public double? DistanceFromRouteStart { get; set; }
    public bool HasAlternativeRoute { get; set; } = false;
    public string? AlternativeRouteDescription { get; set; }
}

public class UpdateRouteTollRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public TollType? Type { get; set; }
    public decimal? BaseCost { get; set; }
    public decimal? CostPerAxle { get; set; }
    public decimal? CostPerWeight { get; set; }
    public string? Currency { get; set; }
    public PaymentMethod? AcceptedPayments { get; set; }
    public bool? IsElectronicTollCollection { get; set; }
    public string? ETCProvider { get; set; }
    public TimeOnly? OperatingHoursStart { get; set; }
    public TimeOnly? OperatingHoursEnd { get; set; }
    public bool? Is24Hours { get; set; }
    public string? SpecialConditions { get; set; }
    public double? DistanceFromRouteStart { get; set; }
    public bool? HasAlternativeRoute { get; set; }
    public string? AlternativeRouteDescription { get; set; }
}