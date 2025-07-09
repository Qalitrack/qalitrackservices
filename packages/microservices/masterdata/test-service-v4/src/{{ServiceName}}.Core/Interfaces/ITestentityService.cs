using TestServiceV4.Core.DTOs;

namespace TestServiceV4.Core.Interfaces;

public interface ITestentityService
{
    Task<IEnumerable<TestentityReadDto>> GetAllAsync();
    Task<TestentityReadDto?> GetByIdAsync(string id);
    Task<TestentityReadDto> CreateAsync(CreateTestentityDto dto);
    Task<TestentityReadDto?> UpdateAsync(string id, UpdateTestentityDto dto);
    Task<bool> DeleteAsync(string id);
    Task<bool> IsNameAvailableAsync(string name);
    
    // TODO: Add domain-specific service methods here
}