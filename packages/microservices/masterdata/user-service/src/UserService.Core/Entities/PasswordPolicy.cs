namespace UserService.Core.Entities;

public class PasswordPolicy: BaseEntity
{
    // Fixed, well-known id for the single policy row — a random
    // Guid.NewGuid() default (from BaseEntity) let two concurrent
    // "first ever save" requests each construct their own object with a
    // different random id and both successfully insert, leaving two rows
    // with no defined "current" one.
    public const string SingletonId = "00000000-0000-0000-0000-000000000e02";

    public int MinimumLength { get; set; }
    public bool RequireUppercase { get; set; }
    public bool RequireLowercase { get; set; }
    public bool RequireDigit { get; set; }
    public bool RequireSpecialCharacter { get; set; }
    public int MaxAgeDays { get; set; }
    public bool TwoFactorEnabled { get; set; }
}