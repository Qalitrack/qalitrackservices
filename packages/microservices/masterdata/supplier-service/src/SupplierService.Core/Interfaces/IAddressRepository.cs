using SupplierService.Core.Entities;

namespace SupplierService.Core.Interfaces;

public interface IAddressRepository : IRepository<Address>
{
    Task<IEnumerable<Address>> GetBySupplierId(string supplierId);
    Task<Address?> GetDefaultAddressAsync(string supplierId);
    Task<bool> SetDefaultAddressAsync(string addressId, string supplierId);
    Task<IEnumerable<Address>> GetByAddressTypeAsync(string supplierId, AddressType type);
}