namespace {{ServiceName}}.Core.DTOs;

public class {{EntityName}}ReadDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // TODO: Add domain-specific properties here
}

public class Create{{EntityName}}Dto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // TODO: Add domain-specific properties here
}

public class Update{{EntityName}}Dto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // TODO: Add domain-specific properties here
}