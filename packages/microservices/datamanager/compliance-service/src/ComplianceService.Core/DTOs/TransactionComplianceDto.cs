namespace ComplianceService.Core.DTOs
{
    public class TransactionComplianceDto
    {
        public string TransactionId { get; set; } = string.Empty;
        public string VehicleId { get; set; } = string.Empty;
        public string DriverId { get; set; } = string.Empty;
        public string RouteId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string OrganizationId { get; set; } = string.Empty;
        public WeightMeasurementDto? WeightMeasurement { get; set; }
        public DriverLicenseDto? DriverLicense { get; set; }
        public VehicleDetailsDto? VehicleDetails { get; set; }
        public RouteDetailsDto? RouteDetails { get; set; }
        public ProductDetailsDto? ProductDetails { get; set; }
        public DateTime TransactionTime { get; set; } = DateTime.UtcNow;
        public string TransactionType { get; set; } = string.Empty; // WEIGHING, ROUTE_CHECK, etc.
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    public class DriverLicenseDto
    {
        public string DriverId { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string LicenseType { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string IssuingAuthority { get; set; } = string.Empty;
    }

    public class VehicleDetailsDto
    {
        public string VehicleId { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public string VehicleClass { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public int AxleCount { get; set; }
        public decimal MaxGrossWeight { get; set; }
        public Dictionary<string, object> Specifications { get; set; } = new();
    }

    public class RouteDetailsDto
    {
        public string RouteId { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public string StartLocation { get; set; } = string.Empty;
        public string EndLocation { get; set; } = string.Empty;
        public List<RouteRestrictionDto> Restrictions { get; set; } = new();
        public Dictionary<string, object> AdditionalInfo { get; set; } = new();
    }

    public class RouteRestrictionDto
    {
        public string RestrictionType { get; set; } = string.Empty;
        public decimal? MaxWeight { get; set; }
        public TimeSpan? RestrictedFromTime { get; set; }
        public TimeSpan? RestrictedToTime { get; set; }
        public string[] AllowedVehicleTypes { get; set; } = Array.Empty<string>();
        public string[] RestrictedProducts { get; set; } = Array.Empty<string>();
    }

    public class ProductDetailsDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public string HazardClass { get; set; } = string.Empty;
        public bool IsHazardous { get; set; }
        public List<string> TransportRestrictions { get; set; } = new();
        public Dictionary<string, object> Properties { get; set; } = new();
    }
}