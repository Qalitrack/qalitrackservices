using CustomerService.Core.DTOs;

namespace CustomerService.Core.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<CustomerReadDto>> GetAllAsync();
    Task<CustomerReadDto?> GetByIdAsync(string id);
    Task<CustomerReadDto> CreateAsync(CreateCustomerDto dto);
    Task<CustomerReadDto?> UpdateAsync(string id, UpdateCustomerDto dto);
    Task<bool> DeleteAsync(string id);
    Task<bool> IsNameAvailableAsync(string name);
    
    // TODO: Add domain-specific service methods here
}