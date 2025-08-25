using SupplierService.Core.DTOs;
using SupplierService.Core.Entities;

namespace SupplierService.Core.Interfaces;

public interface ISupplierService
{
    // Supplier Management
    Task<SupplierDto> RegisterSupplierAsync(RegisterSupplierRequest request);
    Task<SupplierDto> UpdateSupplierAsync(string id, UpdateSupplierRequest request);
    Task<SupplierDto?> GetSupplierByIdAsync(string id);
    Task<SupplierReadDto?> GetByIdAsync(string id);
    Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync();
    Task<IEnumerable<SupplierReadDto>> GetAllAsync();
    Task<IEnumerable<SupplierReadDto>> SearchSuppliersAsync(string searchTerm);
    Task<IEnumerable<SupplierDto>> GetSuppliersByStatusAsync(SupplierStatus status);
    Task<IEnumerable<SupplierDto>> GetSuppliersByTypeAsync(SupplierType type);
    Task DeleteSupplierAsync(string id);
    
    // Additional controller methods (compatible with existing implementations)
    Task<bool> VerifySupplierAsync(string id);
    Task<SupplierReadDto?> GetByCodeAsync(string code);

    // Contact Management
    Task<SupplierContactDto> CreateContactAsync(string supplierId, CreateSupplierContactRequest request);
    Task<SupplierContactDto> UpdateContactAsync(string contactId, UpdateSupplierContactRequest request);
    Task<IEnumerable<SupplierContactDto>> GetSupplierContactsAsync(string supplierId);
    Task<SupplierContactDto?> GetPrimaryContactAsync(string supplierId);
    Task DeleteContactAsync(string contactId);

    // Contract Management
    Task<SupplierContractDto> CreateContractAsync(string supplierId, CreateSupplierContractRequest request);
    Task<SupplierContractDto> UpdateContractAsync(string contractId, UpdateSupplierContractRequest request);
    Task<IEnumerable<SupplierContractDto>> GetSupplierContractsAsync(string supplierId);
    Task<IEnumerable<SupplierContractDto>> GetActiveContractsAsync(string supplierId);
    Task<IEnumerable<SupplierContractDto>> GetExpiringContractsAsync(DateTime date);
    Task DeleteContractAsync(string contractId);

    // Product Management
    Task<SupplierProductDto> CreateProductAsync(string supplierId, CreateSupplierProductRequest request);
    Task<IEnumerable<SupplierProductDto>> GetSupplierProductsAsync(string supplierId);
    Task<IEnumerable<SupplierProductDto>> GetActiveProductsAsync(string supplierId);

    // Performance Management
    Task<SupplierPerformanceDto> CreatePerformanceAsync(string supplierId, CreateSupplierPerformanceRequest request);
    Task<IEnumerable<SupplierPerformanceDto>> GetSupplierPerformanceAsync(string supplierId);
    Task<decimal?> GetAverageRatingAsync(string supplierId, int? months = null);

    // Financial Management
    Task<SupplierFinancialDto> UpdateFinancialAsync(string supplierId, UpdateSupplierFinancialRequest request);
    Task<SupplierFinancialDto?> GetSupplierFinancialAsync(string supplierId);

    // Additional methods needed by SuppliersController
    Task<SupplierReadDto> CreateAsync(CreateSupplierDto dto);
    Task<SupplierReadDto?> UpdateAsync(string id, UpdateSupplierDto dto);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<SupplierReadDto>> GetByStatusAsync(SupplierStatus status);
    Task<IEnumerable<SupplierReadDto>> GetByTypeAsync(SupplierType type);
}