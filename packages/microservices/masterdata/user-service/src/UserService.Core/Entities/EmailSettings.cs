namespace UserService.Core.Entities;

// Singleton pattern - only one row should exist
public class EmailSettings : BaseEntity
{
    // Fixed, well-known id for the single settings row — see
    // PasswordPolicy.SingletonId for why a random Guid.NewGuid() default
    // isn't safe for a singleton row under concurrent first-writes.
    public const string SingletonId = "00000000-0000-0000-0000-000000000e01";

    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; } = "QaliTrack System";
    public bool EnableSsl { get; set; } = true;
}
