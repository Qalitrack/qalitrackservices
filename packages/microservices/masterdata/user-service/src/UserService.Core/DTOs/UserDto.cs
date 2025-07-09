namespace UserService.Core.DTOs;

public class UserReadDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // TODO: Add domain-specific properties here
}

public class CreateUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // TODO: Add domain-specific properties here
}

public class UpdateUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // TODO: Add domain-specific properties here
}