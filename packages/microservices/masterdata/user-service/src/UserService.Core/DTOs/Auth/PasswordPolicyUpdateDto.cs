namespace UserService.Core.DTOs.PasswordPolicy
{
    public class PasswordPolicyUpdateDto
    {
        public int MinimumLength { get; set; }
        public bool RequireUppercase { get; set; }
        public bool RequireLowercase { get; set; }
        public bool RequireDigit { get; set; }
        public bool RequireSpecialCharacter { get; set; }
        public int MaxAgeDays { get; set; }
        public bool TwoFactorEnabled { get; set; }
        }
    }
