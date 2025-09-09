namespace QaliTrack.DataManager.Core.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "system";
    public string UpdatedBy { get; set; } = "system";
    public bool IsDeleted { get; set; } = false;
}

public abstract class TenantEntity : BaseEntity
{
    public Guid OrganizationId { get; set; }
}