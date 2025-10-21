using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Masterdata.Core.DTOs;
using Masterdata.Core.DTOs.Customer;
using Masterdata.Core.Entities;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;
using Microsoft.Extensions.Logging;

namespace Masterdata.Core.Services;

public class CustomerService : ICustomerService
{
    private readonly IRepository<Customer> _customerRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        IRepository<Customer> customerRepository,
        IMapper mapper,
        ILogger<CustomerService> logger)
    {
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<CustomerDto>> GetPagedCustomersAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
    {
        try
        {
            // Ensure page number and size are valid
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            // Get paginated customers with search
            var pagedResult = await _customerRepository.GetPagedAsync(
                pageNumber: pageNumber,
                pageSize: pageSize,
                searchTerm: searchTerm,
                searchProperties: new[] { nameof(Customer.Name) }
            );

            if (!pagedResult.Items.Any())
            {
                return new PagedResult<CustomerDto>
                {
                    Items = Enumerable.Empty<CustomerDto>(),
                    TotalItems = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }

            var items = pagedResult.Items.Select(c => _mapper.Map<CustomerDto>(c)).ToList();

            return new PagedResult<CustomerDto>
            {
                Items = items,
                TotalItems = pagedResult.TotalItems,
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged customers");
            throw;
        }
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var customers = await _customerRepository.GetByIdsAsync(new[] { id.ToString() });
            var customer = customers.FirstOrDefault();
            return customer != null ? _mapper.Map<CustomerDto>(customer) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving customer with ID: {id}");
            throw;
        }
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
    {
        try
        {
            if (await IsNameAvailableAsync(dto.Name) == false)
            {
                throw new InvalidOperationException($"A customer with name '{dto.Name}' already exists.");
            }

            var customer = _mapper.Map<Customer>(dto);
            var createdCustomer = await _customerRepository.CreateAsync(customer);
            return _mapper.Map<CustomerDto>(createdCustomer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer");
            throw;
        }
    }

    public async Task<CustomerDto?> UpdateAsync(Guid id, UpdateCustomerDto dto)
    {
        try
        {
            var customers = await _customerRepository.GetByIdsAsync(new[] { id.ToString() });
            var existingCustomer = customers.FirstOrDefault();
            if (existingCustomer == null)
            {
                return null;
            }

            if (await IsNameAvailableAsync(dto.Name, id) == false)
            {
                throw new InvalidOperationException($"A customer with name '{dto.Name}' already exists.");
            }

            _mapper.Map(dto, existingCustomer);
            var updatedCustomer = await _customerRepository.UpdateAsync(existingCustomer);
            return updatedCustomer != null ? _mapper.Map<CustomerDto>(updatedCustomer) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating customer with ID: {id}");
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            return await _customerRepository.DeleteAsync(id.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting customer with ID: {id}");
            throw;
        }
    }

    public async Task<bool> IsNameAvailableAsync(string name, Guid? excludeId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            // Get all customers with the same name (case-insensitive)
            var result = await _customerRepository.GetPagedAsync(
                pageNumber: 1,
                pageSize: 1,
                searchTerm: name,
                searchProperties: new[] { nameof(Customer.Name) }
            );

            // If no customer found with this name, it's available
            if (!result.Items.Any())
            {
                return true;
            }

            // If checking for a specific customer (update case), exclude it from the check
            if (excludeId.HasValue)
            {
                return result.Items.All(c => c.Id == excludeId.Value.ToString());
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking customer name availability for: {name}");
            throw;
        }
    }
}
