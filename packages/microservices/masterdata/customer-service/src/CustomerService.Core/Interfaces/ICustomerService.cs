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
    
    Task<CustomerReadDto?> ActivateCustomerAsync(string id);
    Task<CustomerReadDto?> DeactivateCustomerAsync(string id);
    Task<CustomerReadDto?> EnableTransporterRoleAsync(string id, string transporterId);
    Task<CustomerReadDto?> SetPreferredTransporterAsync(string id, string transporterId);
}