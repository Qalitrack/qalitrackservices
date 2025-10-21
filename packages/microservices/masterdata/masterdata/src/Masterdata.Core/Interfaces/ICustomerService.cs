using System;
using System.Threading.Tasks;
using Masterdata.Core.DTOs;
using Masterdata.Core.DTOs.Customer;
using Masterdata.Core.Models;

namespace Masterdata.Core.Interfaces;

public interface ICustomerService
{
    Task<PagedResult<CustomerDto>> GetPagedCustomersAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null);
    Task<CustomerDto?> GetByIdAsync(Guid id);
    Task<CustomerDto> CreateAsync(CreateCustomerDto dto);
    Task<CustomerDto?> UpdateAsync(Guid id, UpdateCustomerDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> IsNameAvailableAsync(string name, Guid? excludeId = null);
}
