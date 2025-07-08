namespace MockUserService.Models;

public class MockLoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
}

public class MockLoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserInfo User { get; set; } = null!;
}

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}

public class ValidateTokenRequest
{
    public string Token { get; set; } = string.Empty;
}

public class ValidateTokenResponse
{
    public bool IsValid { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public DateTime ExpiresAt { get; set; }
}

public static class MockRoles
{
    public const string Guest = "Guest";
    public const string User = "User";
    public const string Operator = "Operator";
    public const string Admin = "Admin";
    public const string SuperAdmin = "SuperAdmin";

    public static readonly List<string> AllRoles = new()
    {
        Guest, User, Operator, Admin, SuperAdmin
    };

    public static List<string> GetPermissions(string role)
    {
        return role switch
        {
            Guest => new() { "read:public" },
            User => new() { "read:public", "read:products", "read:profile" },
            Operator => new() { "read:public", "read:products", "read:profile", "write:products", "read:orders" },
            Admin => new() { "read:public", "read:products", "read:profile", "write:products", "read:orders", "write:orders", "delete:products", "manage:users" },
            SuperAdmin => new() { "read:public", "read:products", "read:profile", "write:products", "read:orders", "write:orders", "delete:products", "manage:users", "manage:system", "delete:orders" },
            _ => new() { "read:public" }
        };
    }
}