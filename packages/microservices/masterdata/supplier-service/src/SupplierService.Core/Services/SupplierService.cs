using AutoMapper;
using SupplierService.Core.DTOs;
using SupplierService.Core.Entities;
using SupplierService.Core.Interfaces;

namespace SupplierService.Core.Services;

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IMapper _mapper;

    public SupplierService(
        ISupplierRepository supplierRepository,
        IMapper mapper)
    {
        _supplierRepository = supplierRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SupplierReadDto>> GetAllAsync()
    {
        var suppliers = await _supplierRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<SupplierReadDto>>(suppliers);
    }

    public async Task<SupplierReadDto?> GetByIdAsync(string id)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id);
        return supplier == null ? null : _mapper.Map<SupplierReadDto>(supplier);
    }

    public async Task<SupplierReadDto?> GetByCodeAsync(string code)
    {
        var supplier = await _supplierRepository.GetByCodeAsync(code);
        return supplier == null ? null : _mapper.Map<SupplierReadDto>(supplier);
    }

    public async Task<SupplierReadDto> CreateAsync(CreateSupplierDto dto)
    {
        // Validate uniqueness
        if (!await IsNameAvailableAsync(dto.Name))
            throw new InvalidOperationException($"Supplier with name '{dto.Name}' already exists");

        if (!await IsCodeAvailableAsync(dto.Code))
            throw new InvalidOperationException($"Supplier with code '{dto.Code}' already exists");

        var supplier = _mapper.Map<Supplier>(dto);
        supplier.Status = SupplierStatus.Active;

        var createdSupplier = await _supplierRepository.AddAsync(supplier);
        return _mapper.Map<SupplierReadDto>(createdSupplier);
    }

    public async Task<SupplierReadDto?> UpdateAsync(string id, UpdateSupplierDto dto)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id);
        if (supplier == null)
            return null;

        // Validate name uniqueness if changed
        if (!string.IsNullOrEmpty(dto.Name) && dto.Name != supplier.Name)
        {
            if (!await IsNameAvailableAsync(dto.Name))
                throw new InvalidOperationException($"Supplier with name '{dto.Name}' already exists");
        }

        _mapper.Map(dto, supplier);
        supplier.UpdatedAt = DateTime.UtcNow;

        var updatedSupplier = await _supplierRepository.UpdateAsync(supplier);
        return _mapper.Map<SupplierReadDto>(updatedSupplier);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id);
        if (supplier == null)
            return false;

        await _supplierRepository.DeleteAsync(id);
        return true;
    }

    public async Task<bool> IsNameAvailableAsync(string name)
    {
        return await _supplierRepository.IsNameUniqueAsync(name);
    }

    public async Task<bool> IsCodeAvailableAsync(string code)
    {
        return await _supplierRepository.IsCodeUniqueAsync(code);
    }

    public async Task<IEnumerable<SupplierReadDto>> GetByStatusAsync(SupplierStatus status)
    {
        var suppliers = await _supplierRepository.GetByStatusAsync(status);
        return _mapper.Map<IEnumerable<SupplierReadDto>>(suppliers);
    }

    public async Task<IEnumerable<SupplierReadDto>> GetByTypeAsync(SupplierType type)
    {
        var suppliers = await _supplierRepository.GetByTypeAsync(type);
        return _mapper.Map<IEnumerable<SupplierReadDto>>(suppliers);
    }

    public async Task<IEnumerable<SupplierReadDto>> SearchSuppliersAsync(string searchTerm)
    {
        var suppliers = await _supplierRepository.SearchSuppliersAsync(searchTerm);
        return _mapper.Map<IEnumerable<SupplierReadDto>>(suppliers);
    }

    public async Task<bool> VerifySupplierAsync(string id)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id);
        if (supplier == null)
            return false;

        supplier.IsVerified = true;
        supplier.VerificationDate = DateTime.UtcNow;
        supplier.UpdatedAt = DateTime.UtcNow;

        await _supplierRepository.UpdateAsync(supplier);
        return true;
    }

    // ISupplierService interface methods
    public async Task<SupplierDto> RegisterSupplierAsync(RegisterSupplierRequest request)
    {
        var supplier = _mapper.Map<Supplier>(request);
        var createdSupplier = await _supplierRepository.AddAsync(supplier);
        return _mapper.Map<SupplierDto>(createdSupplier);
    }

    public async Task<SupplierDto> UpdateSupplierAsync(string id, UpdateSupplierRequest request)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id);
        if (supplier == null)
            throw new InvalidOperationException($"Supplier with ID '{id}' not found");

        _mapper.Map(request, supplier);
        supplier.UpdatedAt = DateTime.UtcNow;

        var updatedSupplier = await _supplierRepository.UpdateAsync(supplier);
        return _mapper.Map<SupplierDto>(updatedSupplier);
    }

    public async Task<SupplierDto?> GetSupplierByIdAsync(string id)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id);
        return supplier == null ? null : _mapper.Map<SupplierDto>(supplier);
    }

    public async Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync()
    {
        var suppliers = await _supplierRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<SupplierDto>>(suppliers);
    }

    public async Task<IEnumerable<SupplierDto>> GetSuppliersByStatusAsync(SupplierStatus status)
    {
        var suppliers = await _supplierRepository.GetByStatusAsync(status);
        return _mapper.Map<IEnumerable<SupplierDto>>(suppliers);
    }

    public async Task<IEnumerable<SupplierDto>> GetSuppliersByTypeAsync(SupplierType type)
    {
        var suppliers = await _supplierRepository.GetByTypeAsync(type);
        return _mapper.Map<IEnumerable<SupplierDto>>(suppliers);
    }

    public async Task DeleteSupplierAsync(string id)
    {
        await _supplierRepository.DeleteAsync(id);
    }

    // Contact Management
    public async Task<SupplierContactDto> CreateContactAsync(string supplierId, CreateSupplierContactRequest request)
    {
        throw new NotImplementedException("Contact management requires separate repository");
    }

    public async Task<SupplierContactDto> UpdateContactAsync(string contactId, UpdateSupplierContactRequest request)
    {
        throw new NotImplementedException("Contact management requires separate repository");
    }

    public async Task<IEnumerable<SupplierContactDto>> GetSupplierContactsAsync(string supplierId)
    {
        throw new NotImplementedException("Contact management requires separate repository");
    }

    public async Task<SupplierContactDto?> GetPrimaryContactAsync(string supplierId)
    {
        throw new NotImplementedException("Contact management requires separate repository");
    }

    public async Task DeleteContactAsync(string contactId)
    {
        throw new NotImplementedException("Contact management requires separate repository");
    }

    // Contract Management
    public async Task<SupplierContractDto> CreateContractAsync(string supplierId, CreateSupplierContractRequest request)
    {
        throw new NotImplementedException("Contract management requires separate repository");
    }

    public async Task<SupplierContractDto> UpdateContractAsync(string contractId, UpdateSupplierContractRequest request)
    {
        throw new NotImplementedException("Contract management requires separate repository");
    }

    public async Task<IEnumerable<SupplierContractDto>> GetSupplierContractsAsync(string supplierId)
    {
        throw new NotImplementedException("Contract management requires separate repository");
    }

    public async Task<IEnumerable<SupplierContractDto>> GetActiveContractsAsync(string supplierId)
    {
        throw new NotImplementedException("Contract management requires separate repository");
    }

    public async Task<IEnumerable<SupplierContractDto>> GetExpiringContractsAsync(DateTime date)
    {
        throw new NotImplementedException("Contract management requires separate repository");
    }

    public async Task DeleteContractAsync(string contractId)
    {
        throw new NotImplementedException("Contract management requires separate repository");
    }

    // Product Management
    public async Task<SupplierProductDto> CreateProductAsync(string supplierId, CreateSupplierProductRequest request)
    {
        throw new NotImplementedException("Product management requires separate service");
    }

    public async Task<IEnumerable<SupplierProductDto>> GetSupplierProductsAsync(string supplierId)
    {
        throw new NotImplementedException("Product management requires separate service");
    }

    public async Task<IEnumerable<SupplierProductDto>> GetActiveProductsAsync(string supplierId)
    {
        throw new NotImplementedException("Product management requires separate service");
    }

    // Performance Management
    public async Task<SupplierPerformanceDto> CreatePerformanceAsync(string supplierId, CreateSupplierPerformanceRequest request)
    {
        throw new NotImplementedException("Performance management requires separate service");
    }

    public async Task<IEnumerable<SupplierPerformanceDto>> GetSupplierPerformanceAsync(string supplierId)
    {
        throw new NotImplementedException("Performance management requires separate service");
    }

    public async Task<decimal?> GetAverageRatingAsync(string supplierId, int? months = null)
    {
        throw new NotImplementedException("Performance management requires separate service");
    }

    // Financial Management
    public async Task<SupplierFinancialDto> UpdateFinancialAsync(string supplierId, UpdateSupplierFinancialRequest request)
    {
        throw new NotImplementedException("Financial management requires separate repository");
    }

    public async Task<SupplierFinancialDto?> GetSupplierFinancialAsync(string supplierId)
    {
        throw new NotImplementedException("Financial management requires separate repository");
    }

}