namespace UserService.Core.Utilities
{
    public static class StringUtils
    {
        public static string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains('@'))
                return email;

            var parts = email.Split('@');
            var localPart = parts[0];
            var domain = parts[1];

            if (localPart.Length <= 2)
                return $"{localPart[0]}***@{domain}";

            var maskedLocal = $"{localPart[0]}***{localPart[^1]}";
            return $"{maskedLocal}@{domain}";
        }
    }
}
