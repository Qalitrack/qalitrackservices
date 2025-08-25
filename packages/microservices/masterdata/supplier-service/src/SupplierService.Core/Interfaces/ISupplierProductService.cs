using SupplierService.Core.DTOs;

namespace SupplierService.Core.Interfaces;

public interface ISupplierProductService
{
    Task<SupplierProductDto> CreateProductAsync(string supplierId, CreateSupplierProductRequest request);
    Task<IEnumerable<SupplierProductDto>> GetSupplierProductsAsync(string supplierId);
    Task<IEnumerable<SupplierProductDto>> GetActiveProductsAsync(string supplierId);
    Task<SupplierProductDto?> GetProductByIdAsync(string id);
    Task<SupplierProductDto?> UpdateProductAsync(string id, UpdateSupplierProductDto dto);
    Task<bool> DeleteProductAsync(string id);
    
    // Additional methods expected by controllers
    Task<IEnumerable<SupplierProductDto>> GetAllAsync();
    Task<SupplierProductDto?> GetByIdAsync(string id);
    Task<SupplierProductDto?> UpdateAsync(string id, UpdateSupplierProductDto dto);
    Task<bool> DeleteAsync(string id);
    Task<SupplierProductDto> CreateAsync(CreateSupplierProductDto dto);
    Task<bool> SetPreferredStatusAsync(string id, bool isPreferred);
}