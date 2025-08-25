using {{ServiceName}}.Core.DTOs;

namespace {{ServiceName}}.Core.Interfaces;

public interface I{{EntityName}}Service
{
    Task<IEnumerable<{{EntityName}}ReadDto>> GetAllAsync();
    Task<{{EntityName}}ReadDto?> GetByIdAsync(string id);
    Task<{{EntityName}}ReadDto> CreateAsync(Create{{EntityName}}Dto dto);
    Task<{{EntityName}}ReadDto?> UpdateAsync(string id, Update{{EntityName}}Dto dto);
    Task<bool> DeleteAsync(string id);
    Task<bool> IsNameAvailableAsync(string name);
    
    // TODO: Add domain-specific service methods here
}