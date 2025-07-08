using Microsoft.AspNetCore.Authorization;

namespace UserModule.Authorization;

public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
    {
        Policy = $"RequirePermission:{permission}";
    }
}