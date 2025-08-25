using SupplierService.Core.DTOs;
using SupplierService.Core.Entities;

namespace SupplierService.Core.Interfaces;

public interface IAddressService
{
    Task<IEnumerable<AddressDto>> GetAllAsync(string supplierId);
    Task<AddressDto?> GetByIdAsync(string id);
    Task<AddressDto> CreateAsync(string supplierId, CreateAddressDto dto);
    Task<AddressDto?> UpdateAsync(string id, UpdateAddressDto dto);
    Task<bool> DeleteAsync(string id);
    Task<AddressDto?> GetDefaultAddressAsync(string supplierId);
    Task<bool> SetDefaultAddressAsync(string addressId, string supplierId);
    Task<IEnumerable<AddressDto>> GetByAddressTypeAsync(string supplierId, AddressType type);
}