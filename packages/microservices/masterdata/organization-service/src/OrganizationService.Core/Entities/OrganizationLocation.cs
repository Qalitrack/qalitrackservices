namespace OrganizationService.Core.Entities;

public class OrganizationLocation : BaseEntity
{
    public string OrganizationId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public LocationType Type { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Timezone { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public LocationStatus Status { get; set; }
    public bool IsHeadquarters { get; set; } = false;
    public string? Description { get; set; }
    public string? OperatingHours { get; set; }

    // Navigation properties
    public Organization Organization { get; set; } = null!;
}

public enum LocationType
{
    Headquarters = 0,
    Branch = 1,
    Office = 2,
    Warehouse = 3,
    Factory = 4,
    Store = 5,
    ServiceCenter = 6,
    Other = 7
}

public enum LocationStatus
{
    Active = 0,
    Inactive = 1,
    UnderConstruction = 2,
    Closed = 3
}