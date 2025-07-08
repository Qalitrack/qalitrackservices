using UserService.Core.Entities;

namespace UserService.Core.DTOs;

public class PermissionDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public PermissionScope Scope { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreatePermissionDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public PermissionScope Scope { get; set; } = PermissionScope.System;
}

public class UpdatePermissionDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Resource { get; set; }
    public string? Action { get; set; }
    public PermissionScope? Scope { get; set; }
    public bool? IsActive { get; set; }
}