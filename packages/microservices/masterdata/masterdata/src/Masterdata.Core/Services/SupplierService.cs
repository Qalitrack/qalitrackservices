using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Masterdata.Core.DTOs.Drivers;
using Masterdata.Core.DTOs.Supplier;
using Masterdata.Core.DTOs.Vehicles;
using Masterdata.Core.Entities;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;
using Microsoft.Extensions.Logging;

namespace Masterdata.Core.Services;

public class SupplierService : ISupplierService
{
    private readonly IRepository<Supplier> _repository;
    private readonly IRepository<Driver> _driverRepository;
    private readonly IRepository<Vehicle> _vehicleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<SupplierService> _logger;

    public SupplierService(
        IRepository<Supplier> repository,
        IRepository<Driver> driverRepository,
        IRepository<Vehicle> vehicleRepository,
        IMapper mapper,
        ILogger<SupplierService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _driverRepository = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
        _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<SupplierReadDto>> GetPagedSuppliersAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null)
    {
        var pagedResult = await _repository.GetPagedAsync(
            pageNumber,
            pageSize,
            searchTerm,
            searchTerm != null 
                ? s => (s.Name != null && s.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                       (s.ContactInfo != null && s.ContactInfo.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                : null,
            new[] { nameof(Supplier.Name), nameof(Supplier.ContactInfo) }
        );

        return new PagedResult<SupplierReadDto>
        {
            Items = _mapper.Map<IEnumerable<SupplierReadDto>>(pagedResult.Items),
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            TotalItems = pagedResult.TotalItems
        };
    }

    public async Task<SupplierReadDto?> GetSupplierByIdAsync(string id)
    {
        var suppliers = await _repository.GetByIdsAsync(new[] { id });
        var supplier = suppliers.FirstOrDefault();
        return supplier == null || supplier.IsDeleted ? null : _mapper.Map<SupplierReadDto>(supplier);
    }

    public async Task<SupplierReadDto> CreateSupplierAsync(CreateSupplierDto dto)
    {
        var supplier = _mapper.Map<Supplier>(dto);
        var created = await _repository.CreateAsync(supplier);
        
        if (created == null)
        {
            throw new InvalidOperationException("Failed to create supplier");
        }
        
        return _mapper.Map<SupplierReadDto>(created);
    }

    public async Task<bool> UpdateSupplierAsync(string id, UpdateSupplierDto dto)
    {
        var suppliers = await _repository.GetByIdsAsync(new[] { id });
        var supplier = suppliers.FirstOrDefault();
        
        if (supplier == null || supplier.IsDeleted)
        {
            return false;
        }

        _mapper.Map(dto, supplier);
        var updated = await _repository.UpdateAsync(supplier);
        return updated != null;
    }

    public async Task<bool> DeleteSupplierAsync(string id)
    {
        var suppliers = await _repository.GetByIdsAsync(new[] { id });
        var supplier = suppliers.FirstOrDefault();
        
        if (supplier == null || supplier.IsDeleted)
        {
            return false;
        }

        return await _repository.DeleteAsync(id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var suppliers = await _repository.GetByIdsAsync(new[] { id });
        var supplier = suppliers.FirstOrDefault();
        return supplier != null && !supplier.IsDeleted;
    }

    public async Task<IEnumerable<SupplierReadDto>> GetSuppliersByIdsAsync(IEnumerable<string> ids)
    {
        var uniqueIds = ids.Distinct().ToList();
        var suppliers = await _repository.GetByIdsAsync(uniqueIds);
        return _mapper.Map<IEnumerable<SupplierReadDto>>(suppliers.Where(s => !s.IsDeleted));
    }

    public async Task<IEnumerable<DriverReadDto>> GetDriversBySupplierIdAsync(string supplierId)
    {
        var supplier = (await _repository.GetByIdsAsync(new[] { supplierId })).FirstOrDefault();
        if (supplier == null || supplier.IsDeleted)
        {
            return Enumerable.Empty<DriverReadDto>();
        }

        // Use a large page size to get all matching drivers
        var result = await _driverRepository.GetPagedAsync(
            pageNumber: 1,
            pageSize: int.MaxValue,
            searchPredicate: d => d.SupplierId == supplierId && !d.IsDeleted);
            
        return _mapper.Map<IEnumerable<DriverReadDto>>(result.Items);
    }

    public async Task<IEnumerable<VehicleReadDto>> GetVehiclesBySupplierIdAsync(string supplierId)
    {
        var supplier = (await _repository.GetByIdsAsync(new[] { supplierId })).FirstOrDefault();
        if (supplier == null || supplier.IsDeleted)
        {
            return Enumerable.Empty<VehicleReadDto>();
        }

        // Use a large page size to get all matching vehicles
        var result = await _vehicleRepository.GetPagedAsync(
            pageNumber: 1,
            pageSize: int.MaxValue,
            searchPredicate: v => v.SupplierId == supplierId && !v.IsDeleted);
            
        return _mapper.Map<IEnumerable<VehicleReadDto>>(result.Items);
    }
}
