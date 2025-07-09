namespace MockUserService.Models;

public class MockLoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
}

public class MockLoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;  // Primary role (backward compatibility)
    public List<string> Roles { get; set; } = new();  // All roles (future multi-role support)
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
    public string Role { get; set; } = string.Empty;  // Primary role (backward compatibility)
    public List<string> Roles { get; set; } = new();  // All roles (future multi-role support)
    public List<string> Permissions { get; set; } = new();  // Computed permissions from roles
}

public class ValidateTokenRequest
{
    public string Token { get; set; } = string.Empty;
}

public class ValidateTokenResponse
{
    public bool IsValid { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;  // Primary role (backward compatibility)
    public List<string> Roles { get; set; } = new();  // All roles
    public List<string> Permissions { get; set; } = new();  // All permissions
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
            User => new() { 
                "read:public", 
                "read:products", 
                "read:profile" 
            },
            Operator => new() { 
                "read:public", 
                "read:products", 
                "read:profile", 
                "write:products", 
                "read:customers", 
                "write:customers",
                "read:vehicles",
                "write:vehicles",
                "read:drivers",
                "write:drivers",
                "read:suppliers",
                "write:suppliers",
                "read:weight-data",
                "write:weight-data"
            },
            Admin => new() { 
                "read:public", 
                "read:products", 
                "read:profile", 
                "write:products", 
                "delete:products",
                "read:customers", 
                "write:customers",
                "delete:customers",
                "read:vehicles",
                "write:vehicles", 
                "delete:vehicles",
                "read:drivers",
                "write:drivers",
                "delete:drivers",
                "read:suppliers",
                "write:suppliers",
                "delete:suppliers",
                "read:weight-data",
                "write:weight-data",
                "delete:weight-data",
                "read:users",
                "write:users",
                "manage:users",
                "read:organizations",
                "write:organizations"
            },
            SuperAdmin => new() { 
                "read:public", 
                "read:products", 
                "read:profile", 
                "write:products", 
                "delete:products",
                "read:customers", 
                "write:customers",
                "delete:customers",
                "read:vehicles",
                "write:vehicles", 
                "delete:vehicles",
                "read:drivers",
                "write:drivers",
                "delete:drivers",
                "read:suppliers",
                "write:suppliers",
                "delete:suppliers",
                "read:weight-data",
                "write:weight-data",
                "delete:weight-data",
                "read:users",
                "write:users",
                "manage:users",
                "delete:users",
                "read:organizations",
                "write:organizations",
                "manage:organizations",
                "delete:organizations",
                "manage:system",
                "read:analytics",
                "read:compliance",
                "manage:compliance"
            },
            _ => new() { "read:public" }
        };
    }

    /// <summary>
    /// Get roles hierarchy - a role inherits all permissions from lower-level roles
    /// </summary>
    public static List<string> GetRolesHierarchy(string primaryRole)
    {
        return primaryRole switch
        {
            SuperAdmin => new() { SuperAdmin, Admin, Operator, User, Guest },
            Admin => new() { Admin, Operator, User, Guest },
            Operator => new() { Operator, User, Guest },
            User => new() { User, Guest },
            Guest => new() { Guest },
            _ => new() { Guest }
        };
    }

    /// <summary>
    /// Get role level for hierarchy comparison
    /// </summary>
    public static int GetRoleLevel(string role)
    {
        return role switch
        {
            Guest => 0,
            User => 1,
            Operator => 2,
            Admin => 4,
            SuperAdmin => 5,
            _ => 0
        };
    }
}