using System.ComponentModel.DataAnnotations;

namespace RouteService.Core.Entities;

public class RouteToll : BaseEntity
{
    [Required]
    public string RouteId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? Description { get; set; }
    
    [Required]
    [Range(-90, 90)]
    public double Latitude { get; set; }
    
    [Required]
    [Range(-180, 180)]
    public double Longitude { get; set; }
    
    [Required]
    public TollType Type { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal BaseCost { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? CostPerAxle { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? CostPerWeight { get; set; }
    
    [StringLength(10)]
    public string Currency { get; set; } = "USD";
    
    [Required]
    public PaymentMethod AcceptedPayments { get; set; } = PaymentMethod.Cash;
    
    public bool IsElectronicTollCollection { get; set; } = false;
    
    [StringLength(50)]
    public string? ETCProvider { get; set; }
    
    public TimeOnly? OperatingHoursStart { get; set; }
    public TimeOnly? OperatingHoursEnd { get; set; }
    
    public bool Is24Hours { get; set; } = true;
    
    [StringLength(500)]
    public string? SpecialConditions { get; set; }
    
    [Range(0, double.MaxValue)]
    public double? DistanceFromRouteStart { get; set; }
    
    public bool HasAlternativeRoute { get; set; } = false;
    
    [StringLength(200)]
    public string? AlternativeRouteDescription { get; set; }
    
    // Navigation property
    public virtual Route Route { get; set; } = null!;
}

public enum TollType
{
    Bridge,
    Tunnel,
    Highway,
    Road,
    Plaza,
    Booth,
    Electronic,
    Congestion
}

[Flags]
public enum PaymentMethod
{
    Cash = 1,
    CreditCard = 2,
    DebitCard = 4,
    ETC = 8, // Electronic Toll Collection
    Mobile = 16,
    Prepaid = 32
}