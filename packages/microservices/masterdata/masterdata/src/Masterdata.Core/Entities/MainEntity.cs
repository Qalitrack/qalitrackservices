namespace Masterdata.Core.Entities;

public class Base : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public BaseStatus Status { get; set; } = BaseStatus.Active;
    
}

public enum BaseStatus
{
    Active,
    Inactive,
    Suspended,
    Pending
}