using AutoMapper;
using CustomerService.Core.DTOs;
using CustomerService.Core.Entities;
using CustomerService.Core.Interfaces;

namespace CustomerService.Core.Services;

public class CustomerManagementService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IRepository<Contact> _contactRepository;
    private readonly IRepository<Contract> _contractRepository;
    private readonly IMapper _mapper;

    public CustomerManagementService(
        ICustomerRepository customerRepository,
        IRepository<Contact> contactRepository,
        IRepository<Contract> contractRepository,
        IMapper mapper)
    {
        _customerRepository = customerRepository;
        _contactRepository = contactRepository;
        _contractRepository = contractRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CustomerReadDto>> GetAllAsync()
    {
        var customers = await _customerRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CustomerReadDto>>(customers);
    }

    public async Task<CustomerReadDto?> GetByIdAsync(string id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        return customer == null ? null : _mapper.Map<CustomerReadDto>(customer);
    }

    public async Task<CustomerReadDto> CreateAsync(CreateCustomerDto dto)
    {
        // Validate unique constraints
        if (!string.IsNullOrEmpty(dto.ContactEmail))
        {
            var existingByEmail = await _customerRepository.GetByEmailAsync(dto.ContactEmail);
            if (existingByEmail != null)
            {
                throw new InvalidOperationException($"Customer with email {dto.ContactEmail} already exists");
            }
        }

        if (!string.IsNullOrEmpty(dto.TaxNumber))
        {
            var existingByTax = await _customerRepository.GetByTaxNumberAsync(dto.TaxNumber);
            if (existingByTax != null)
            {
                throw new InvalidOperationException($"Customer with tax number {dto.TaxNumber} already exists");
            }
        }

        var customer = _mapper.Map<Customer>(dto);
        customer.CreatedAt = DateTime.UtcNow;
        customer.UpdatedAt = DateTime.UtcNow;
        
        var createdCustomer = await _customerRepository.CreateAsync(customer);
        return _mapper.Map<CustomerReadDto>(createdCustomer);
    }

    public async Task<CustomerReadDto?> UpdateAsync(string id, UpdateCustomerDto dto)
    {
        var existingCustomer = await _customerRepository.GetByIdAsync(id);
        if (existingCustomer == null)
        {
            return null;
        }

        // Validate unique constraints if changed
        if (!string.IsNullOrEmpty(dto.ContactEmail) && dto.ContactEmail != existingCustomer.ContactEmail)
        {
            var existingByEmail = await _customerRepository.GetByEmailAsync(dto.ContactEmail);
            if (existingByEmail != null && existingByEmail.Id != id)
            {
                throw new InvalidOperationException($"Customer with email {dto.ContactEmail} already exists");
            }
        }

        _mapper.Map(dto, existingCustomer);
        existingCustomer.UpdatedAt = DateTime.UtcNow;
        
        var updatedCustomer = await _customerRepository.UpdateAsync(existingCustomer);
        return updatedCustomer == null ? null : _mapper.Map<CustomerReadDto>(updatedCustomer);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _customerRepository.DeleteAsync(id);
    }

    public async Task<bool> IsNameAvailableAsync(string name)
    {
        return await _customerRepository.IsNameAvailableAsync(name);
    }

    public async Task<CustomerReadDto?> ActivateCustomerAsync(string id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null) return null;

        customer.Status = CustomerStatus.Active;
        customer.UpdatedAt = DateTime.UtcNow;
        var updatedCustomer = await _customerRepository.UpdateAsync(customer);
        return updatedCustomer == null ? null : _mapper.Map<CustomerReadDto>(updatedCustomer);
    }

    public async Task<CustomerReadDto?> DeactivateCustomerAsync(string id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null) return null;

        customer.Status = CustomerStatus.Inactive;
        customer.UpdatedAt = DateTime.UtcNow;
        var updatedCustomer = await _customerRepository.UpdateAsync(customer);
        return updatedCustomer == null ? null : _mapper.Map<CustomerReadDto>(updatedCustomer);
    }

    public async Task<CustomerReadDto?> EnableTransporterRoleAsync(string id, string transporterId)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null) return null;

        // Enable transporter role for this customer
        customer.TransporterId = transporterId;
        customer.UpdatedAt = DateTime.UtcNow;
        var updatedCustomer = await _customerRepository.UpdateAsync(customer);
        return updatedCustomer == null ? null : _mapper.Map<CustomerReadDto>(updatedCustomer);
    }

    public async Task<CustomerReadDto?> SetPreferredTransporterAsync(string id, string transporterId)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null) return null;

        customer.PreferredTransporterId = transporterId;
        customer.UpdatedAt = DateTime.UtcNow;
        var updatedCustomer = await _customerRepository.UpdateAsync(customer);
        return updatedCustomer == null ? null : _mapper.Map<CustomerReadDto>(updatedCustomer);
    }
}