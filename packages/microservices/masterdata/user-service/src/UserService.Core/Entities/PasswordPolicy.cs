namespace UserService.Core.Entities;

public class PasswordPolicy: BaseEntity
{
    public int MinimumLength { get; set; }
    public bool RequireUppercase { get; set; }
    public bool RequireLowercase { get; set; }
    public bool RequireDigit { get; set; }
    public bool RequireSpecialCharacter { get; set; }
    public int MaxAgeDays { get; set; }
}