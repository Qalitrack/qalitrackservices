using AutoMapper;
using SupplierService.Core.DTOs;
using SupplierService.Core.Entities;
using SupplierService.Core.Interfaces;

namespace SupplierService.Core.Services;

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly ISupplierContactRepository _contactRepository;
    private readonly ISupplierContractRepository _contractRepository;
    private readonly ISupplierProductRepository _productRepository;
    private readonly ISupplierPerformanceRepository _performanceRepository;
    private readonly ISupplierFinancialRepository _financialRepository;
    private readonly IMapper _mapper;

    public SupplierService(
        ISupplierRepository supplierRepository,
        ISupplierContactRepository contactRepository,
        ISupplierContractRepository contractRepository,
        ISupplierProductRepository productRepository,
        ISupplierPerformanceRepository performanceRepository,
        ISupplierFinancialRepository financialRepository,
        IMapper mapper)
    {
        _supplierRepository = supplierRepository;
        _contactRepository = contactRepository;
        _contractRepository = contractRepository;
        _productRepository = productRepository;
        _performanceRepository = performanceRepository;
        _financialRepository = financialRepository;
        _mapper = mapper;
    }

    // Supplier Management
    public async Task<SupplierDto> RegisterSupplierAsync(RegisterSupplierRequest request)
    {
        // Check for duplicate by name
        var existingByName = await _supplierRepository.GetByNameAsync(request.Name);
        if (existingByName != null)
            throw new InvalidOperationException($"Supplier with name '{request.Name}' already exists");

        // Check for duplicate by tax number if provided
        if (!string.IsNullOrEmpty(request.TaxNumber))
        {
            var existingByTax = await _supplierRepository.GetByTaxNumberAsync(request.TaxNumber);
            if (existingByTax != null)
                throw new InvalidOperationException($"Supplier with tax number '{request.TaxNumber}' already exists");
        }

        var supplier = _mapper.Map<Supplier>(request);
        supplier.Status = SupplierStatus.Active;

        var createdSupplier = await _supplierRepository.AddAsync(supplier);
        return _mapper.Map<SupplierDto>(createdSupplier);
    }

    public async Task<SupplierDto> UpdateSupplierAsync(string id, UpdateSupplierRequest request)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id);
        if (supplier == null)
            throw new KeyNotFoundException($"Supplier with ID '{id}' not found");

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

    public async Task<IEnumerable<SupplierDto>> SearchSuppliersAsync(string searchTerm)
    {
        var suppliers = await _supplierRepository.SearchAsync(searchTerm);
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
        var supplier = await _supplierRepository.GetByIdAsync(id);
        if (supplier == null)
            throw new KeyNotFoundException($"Supplier with ID '{id}' not found");

        await _supplierRepository.DeleteAsync(id);
    }

    // Contact Management
    public async Task<SupplierContactDto> CreateContactAsync(string supplierId, CreateSupplierContactRequest request)
    {
        var supplier = await _supplierRepository.GetByIdAsync(supplierId);
        if (supplier == null)
            throw new KeyNotFoundException($"Supplier with ID '{supplierId}' not found");

        var contact = _mapper.Map<SupplierContact>(request);
        contact.SupplierId = supplierId;

        var createdContact = await _contactRepository.AddAsync(contact);
        return _mapper.Map<SupplierContactDto>(createdContact);
    }

    public async Task<SupplierContactDto> UpdateContactAsync(string contactId, UpdateSupplierContactRequest request)
    {
        var contact = await _contactRepository.GetByIdAsync(contactId);
        if (contact == null)
            throw new KeyNotFoundException($"Contact with ID '{contactId}' not found");

        _mapper.Map(request, contact);
        contact.UpdatedAt = DateTime.UtcNow;

        var updatedContact = await _contactRepository.UpdateAsync(contact);
        return _mapper.Map<SupplierContactDto>(updatedContact);
    }

    public async Task<IEnumerable<SupplierContactDto>> GetSupplierContactsAsync(string supplierId)
    {
        var contacts = await _contactRepository.GetBySupplierIdAsync(supplierId);
        return _mapper.Map<IEnumerable<SupplierContactDto>>(contacts);
    }

    public async Task<SupplierContactDto?> GetPrimaryContactAsync(string supplierId)
    {
        var contact = await _contactRepository.GetPrimaryContactAsync(supplierId);
        return contact == null ? null : _mapper.Map<SupplierContactDto>(contact);
    }

    public async Task DeleteContactAsync(string contactId)
    {
        var contact = await _contactRepository.GetByIdAsync(contactId);
        if (contact == null)
            throw new KeyNotFoundException($"Contact with ID '{contactId}' not found");

        await _contactRepository.DeleteAsync(contactId);
    }

    // Contract Management
    public async Task<SupplierContractDto> CreateContractAsync(string supplierId, CreateSupplierContractRequest request)
    {
        var supplier = await _supplierRepository.GetByIdAsync(supplierId);
        if (supplier == null)
            throw new KeyNotFoundException($"Supplier with ID '{supplierId}' not found");

        // Check for duplicate contract number
        var existingContract = await _contractRepository.GetByContractNumberAsync(request.ContractNumber);
        if (existingContract != null)
            throw new InvalidOperationException($"Contract with number '{request.ContractNumber}' already exists");

        var contract = _mapper.Map<SupplierContract>(request);
        contract.SupplierId = supplierId;

        var createdContract = await _contractRepository.AddAsync(contract);
        return _mapper.Map<SupplierContractDto>(createdContract);
    }

    public async Task<SupplierContractDto> UpdateContractAsync(string contractId, UpdateSupplierContractRequest request)
    {
        var contract = await _contractRepository.GetByIdAsync(contractId);
        if (contract == null)
            throw new KeyNotFoundException($"Contract with ID '{contractId}' not found");

        _mapper.Map(request, contract);
        contract.UpdatedAt = DateTime.UtcNow;

        var updatedContract = await _contractRepository.UpdateAsync(contract);
        return _mapper.Map<SupplierContractDto>(updatedContract);
    }

    public async Task<IEnumerable<SupplierContractDto>> GetSupplierContractsAsync(string supplierId)
    {
        var contracts = await _contractRepository.GetBySupplierIdAsync(supplierId);
        return _mapper.Map<IEnumerable<SupplierContractDto>>(contracts);
    }

    public async Task<IEnumerable<SupplierContractDto>> GetActiveContractsAsync(string supplierId)
    {
        var contracts = await _contractRepository.GetActiveContractsAsync(supplierId);
        return _mapper.Map<IEnumerable<SupplierContractDto>>(contracts);
    }

    public async Task<IEnumerable<SupplierContractDto>> GetExpiringContractsAsync(DateTime date)
    {
        var contracts = await _contractRepository.GetExpiringContractsAsync(date);
        return _mapper.Map<IEnumerable<SupplierContractDto>>(contracts);
    }

    public async Task DeleteContractAsync(string contractId)
    {
        var contract = await _contractRepository.GetByIdAsync(contractId);
        if (contract == null)
            throw new KeyNotFoundException($"Contract with ID '{contractId}' not found");

        await _contractRepository.DeleteAsync(contractId);
    }

    // Product Management
    public async Task<SupplierProductDto> CreateProductAsync(string supplierId, CreateSupplierProductRequest request)
    {
        var supplier = await _supplierRepository.GetByIdAsync(supplierId);
        if (supplier == null)
            throw new KeyNotFoundException($"Supplier with ID '{supplierId}' not found");

        var product = _mapper.Map<SupplierProduct>(request);
        product.SupplierId = supplierId;

        var createdProduct = await _productRepository.AddAsync(product);
        return _mapper.Map<SupplierProductDto>(createdProduct);
    }

    public async Task<IEnumerable<SupplierProductDto>> GetSupplierProductsAsync(string supplierId)
    {
        var products = await _productRepository.GetBySupplierIdAsync(supplierId);
        return _mapper.Map<IEnumerable<SupplierProductDto>>(products);
    }

    public async Task<IEnumerable<SupplierProductDto>> GetActiveProductsAsync(string supplierId)
    {
        var products = await _productRepository.GetActiveProductsAsync(supplierId);
        return _mapper.Map<IEnumerable<SupplierProductDto>>(products);
    }

    // Performance Management
    public async Task<SupplierPerformanceDto> CreatePerformanceAsync(string supplierId, CreateSupplierPerformanceRequest request)
    {
        var supplier = await _supplierRepository.GetByIdAsync(supplierId);
        if (supplier == null)
            throw new KeyNotFoundException($"Supplier with ID '{supplierId}' not found");

        // Check if performance already exists for this period
        var existing = await _performanceRepository.GetBySupplierAndPeriodAsync(supplierId, request.Year, request.Month);
        if (existing != null)
            throw new InvalidOperationException($"Performance record already exists for {request.Month}/{request.Year}");

        var performance = _mapper.Map<SupplierPerformance>(request);
        performance.SupplierId = supplierId;

        // Calculate derived metrics
        if (performance.TotalOrders > 0)
        {
            performance.OnTimeDeliveryRate = (decimal)performance.OnTimeDeliveries / performance.TotalOrders * 100;
            performance.DefectRate = (decimal)performance.DefectiveDeliveries / performance.TotalOrders * 100;
        }

        var createdPerformance = await _performanceRepository.AddAsync(performance);
        return _mapper.Map<SupplierPerformanceDto>(createdPerformance);
    }

    public async Task<IEnumerable<SupplierPerformanceDto>> GetSupplierPerformanceAsync(string supplierId)
    {
        var performance = await _performanceRepository.GetBySupplierIdAsync(supplierId);
        return _mapper.Map<IEnumerable<SupplierPerformanceDto>>(performance);
    }

    public async Task<decimal?> GetAverageRatingAsync(string supplierId, int? months = null)
    {
        return await _performanceRepository.GetAverageRatingAsync(supplierId, months);
    }

    // Financial Management
    public async Task<SupplierFinancialDto> UpdateFinancialAsync(string supplierId, UpdateSupplierFinancialRequest request)
    {
        var supplier = await _supplierRepository.GetByIdAsync(supplierId);
        if (supplier == null)
            throw new KeyNotFoundException($"Supplier with ID '{supplierId}' not found");

        var financial = await _financialRepository.GetBySupplierIdAsync(supplierId);
        if (financial == null)
        {
            financial = new SupplierFinancial { SupplierId = supplierId };
            _mapper.Map(request, financial);
            financial = await _financialRepository.AddAsync(financial);
        }
        else
        {
            _mapper.Map(request, financial);
            financial.UpdatedAt = DateTime.UtcNow;
            financial = await _financialRepository.UpdateAsync(financial);
        }

        return _mapper.Map<SupplierFinancialDto>(financial);
    }

    public async Task<SupplierFinancialDto?> GetSupplierFinancialAsync(string supplierId)
    {
        var financial = await _financialRepository.GetBySupplierIdAsync(supplierId);
        return financial == null ? null : _mapper.Map<SupplierFinancialDto>(financial);
    }
}