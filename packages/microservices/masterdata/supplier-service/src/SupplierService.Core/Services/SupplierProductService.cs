using AutoMapper;
using SupplierService.Core.DTOs;
using SupplierService.Core.Entities;
using SupplierService.Core.Interfaces;

namespace SupplierService.Core.Services;

public class SupplierProductService : ISupplierProductService
{
    private readonly ISupplierProductRepository _supplierProductRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IMapper _mapper;

    public SupplierProductService(
        ISupplierProductRepository supplierProductRepository,
        ISupplierRepository supplierRepository,
        IMapper mapper)
    {
        _supplierProductRepository = supplierProductRepository;
        _supplierRepository = supplierRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SupplierProductDto>> GetAllAsync(string supplierId)
    {
        var products = await _supplierProductRepository.GetBySupplierId(supplierId);
        return _mapper.Map<IEnumerable<SupplierProductDto>>(products);
    }

    public async Task<SupplierProductDto?> GetByIdAsync(string id)
    {
        var product = await _supplierProductRepository.GetByIdAsync(id);
        return product == null ? null : _mapper.Map<SupplierProductDto>(product);
    }

    public async Task<SupplierProductDto?> GetBySupplierAndProductIdAsync(string supplierId, string productId)
    {
        var product = await _supplierProductRepository.GetBySupplierAndProductId(supplierId, productId);
        return product == null ? null : _mapper.Map<SupplierProductDto>(product);
    }

    public async Task<SupplierProductDto> CreateAsync(string supplierId, CreateSupplierProductDto dto)
    {
        // Verify supplier exists
        var supplier = await _supplierRepository.GetByIdAsync(supplierId);
        if (supplier == null)
            throw new InvalidOperationException($"Supplier with ID '{supplierId}' not found");

        // Check if product already exists for this supplier
        var existing = await _supplierProductRepository.GetBySupplierAndProductId(supplierId, dto.ProductId);
        if (existing != null)
            throw new InvalidOperationException($"Product '{dto.ProductId}' already exists for supplier '{supplierId}'");

        var supplierProduct = _mapper.Map<SupplierProduct>(dto);
        supplierProduct.SupplierId = supplierId;

        var createdProduct = await _supplierProductRepository.AddAsync(supplierProduct);
        return _mapper.Map<SupplierProductDto>(createdProduct);
    }

    public async Task<SupplierProductDto?> UpdateAsync(string id, UpdateSupplierProductDto dto)
    {
        var product = await _supplierProductRepository.GetByIdAsync(id);
        if (product == null)
            return null;

        _mapper.Map(dto, product);
        product.UpdatedAt = DateTime.UtcNow;

        var updatedProduct = await _supplierProductRepository.UpdateAsync(product);
        return _mapper.Map<SupplierProductDto>(updatedProduct);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var product = await _supplierProductRepository.GetByIdAsync(id);
        if (product == null)
            return false;

        await _supplierProductRepository.DeleteAsync(id);
        return true;
    }

    public async Task<IEnumerable<SupplierProductDto>> GetByAvailabilityStatusAsync(string supplierId, ProductAvailabilityStatus status)
    {
        var products = await _supplierProductRepository.GetBySupplierId(supplierId);
        var filteredProducts = products.Where(p => p.Status == status);
        return _mapper.Map<IEnumerable<SupplierProductDto>>(filteredProducts);
    }

    public async Task<IEnumerable<SupplierProductDto>> GetPreferredProductsAsync(string supplierId)
    {
        var products = await _supplierProductRepository.GetPreferredProducts(supplierId);
        return _mapper.Map<IEnumerable<SupplierProductDto>>(products);
    }

    public async Task<IEnumerable<SupplierProductDto>> GetLowStockProductsAsync(string supplierId, int threshold = 0)
    {
        var products = await _supplierProductRepository.GetLowStockProducts(supplierId, threshold);
        return _mapper.Map<IEnumerable<SupplierProductDto>>(products);
    }

    public async Task<bool> UpdateStockAsync(string id, int quantity)
    {
        var product = await _supplierProductRepository.GetByIdAsync(id);
        if (product == null)
            return false;

        product.CurrentStock = quantity;
        product.LastRestockDate = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        await _supplierProductRepository.UpdateAsync(product);
        return true;
    }

    public async Task<bool> SetPreferredStatusAsync(string id, bool isPreferred)
    {
        var product = await _supplierProductRepository.GetByIdAsync(id);
        if (product == null)
            return false;

        product.IsPreferred = isPreferred;
        product.UpdatedAt = DateTime.UtcNow;

        await _supplierProductRepository.UpdateAsync(product);
        return true;
    }
}