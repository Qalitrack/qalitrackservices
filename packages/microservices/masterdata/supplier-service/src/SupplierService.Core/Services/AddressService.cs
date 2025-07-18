using AutoMapper;
using SupplierService.Core.DTOs;
using SupplierService.Core.Entities;
using SupplierService.Core.Interfaces;

namespace SupplierService.Core.Services;

public class AddressService : IAddressService
{
    private readonly IAddressRepository _addressRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IMapper _mapper;

    public AddressService(
        IAddressRepository addressRepository,
        ISupplierRepository supplierRepository,
        IMapper mapper)
    {
        _addressRepository = addressRepository;
        _supplierRepository = supplierRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AddressDto>> GetAllAsync(string supplierId)
    {
        var addresses = await _addressRepository.GetBySupplierId(supplierId);
        return _mapper.Map<IEnumerable<AddressDto>>(addresses);
    }

    public async Task<AddressDto?> GetByIdAsync(string id)
    {
        var address = await _addressRepository.GetByIdAsync(id);
        return address == null ? null : _mapper.Map<AddressDto>(address);
    }

    public async Task<AddressDto> CreateAsync(string supplierId, CreateAddressDto dto)
    {
        // Verify supplier exists
        var supplier = await _supplierRepository.GetByIdAsync(supplierId);
        if (supplier == null)
            throw new InvalidOperationException($"Supplier with ID '{supplierId}' not found");

        var address = _mapper.Map<Address>(dto);
        address.SupplierId = supplierId;

        // If this is the first address or marked as default, make it default
        var existingAddresses = await _addressRepository.GetBySupplierId(supplierId);
        if (!existingAddresses.Any() || dto.IsDefault)
        {
            address.IsDefault = true;
        }

        var createdAddress = await _addressRepository.AddAsync(address);

        // If this address is set as default, update other addresses
        if (address.IsDefault)
        {
            await _addressRepository.SetDefaultAddressAsync(createdAddress.Id, supplierId);
        }

        return _mapper.Map<AddressDto>(createdAddress);
    }

    public async Task<AddressDto?> UpdateAsync(string id, UpdateAddressDto dto)
    {
        var address = await _addressRepository.GetByIdAsync(id);
        if (address == null)
            return null;

        _mapper.Map(dto, address);
        address.UpdatedAt = DateTime.UtcNow;

        var updatedAddress = await _addressRepository.UpdateAsync(address);

        // If this address is set as default, update other addresses
        if (dto.IsDefault == true)
        {
            await _addressRepository.SetDefaultAddressAsync(id, address.SupplierId);
        }

        return _mapper.Map<AddressDto>(updatedAddress);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var address = await _addressRepository.GetByIdAsync(id);
        if (address == null)
            return false;

        await _addressRepository.DeleteAsync(id);
        return true;
    }

    public async Task<AddressDto?> GetDefaultAddressAsync(string supplierId)
    {
        var address = await _addressRepository.GetDefaultAddressAsync(supplierId);
        return address == null ? null : _mapper.Map<AddressDto>(address);
    }

    public async Task<bool> SetDefaultAddressAsync(string addressId, string supplierId)
    {
        return await _addressRepository.SetDefaultAddressAsync(addressId, supplierId);
    }

    public async Task<IEnumerable<AddressDto>> GetByAddressTypeAsync(string supplierId, AddressType type)
    {
        var addresses = await _addressRepository.GetByAddressTypeAsync(supplierId, type);
        return _mapper.Map<IEnumerable<AddressDto>>(addresses);
    }
}