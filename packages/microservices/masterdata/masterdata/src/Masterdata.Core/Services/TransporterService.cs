using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Masterdata.Core.DTOs.Drivers;
using Masterdata.Core.DTOs.Transporters;
using Masterdata.Core.DTOs.Vehicles;
using Masterdata.Core.Entities;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;
using Microsoft.Extensions.Logging;

namespace Masterdata.Core.Services;

public class TransporterService : ITransporterService
{
    private readonly IRepository<Transporter> _repository;
    private readonly IRepository<Vehicle> _vehicleRepository;
    private readonly IRepository<Driver> _driverRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<TransporterService> _logger;

    public TransporterService(
        IRepository<Transporter> repository,
        IRepository<Vehicle> vehicleRepository,
        IRepository<Driver> driverRepository,
        IMapper mapper,
        ILogger<TransporterService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
        _driverRepository = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<TransporterReadDto>> GetPagedTransportersAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null)
    {
        // Frontend has callers requesting up to 500 (the Transporters
        // management page loads the full list client-side) — clamp guards
        // against a pathological pageSize without breaking that.
        pageSize = Math.Clamp(pageSize, 1, 500);

        // Note: ContactInfo is stored as a jsonb column, so it can't be matched with a
        // plain-text LIKE/lower() comparison (Postgres has no lower(jsonb) overload) -
        // search is limited to Name.
        var pagedResult = await _repository.GetPagedAsync(
            pageNumber,
            pageSize,
            searchTerm,
            searchTerm != null
                ? t => t.Name.ToLower().Contains(searchTerm.ToLower())
                : null,
            new[] { nameof(Transporter.Name) }
        );

        return new PagedResult<TransporterReadDto>
        {
            Items = _mapper.Map<IEnumerable<TransporterReadDto>>(pagedResult.Items),
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            TotalItems = pagedResult.TotalItems
        };
    }

    public async Task<TransporterReadDto?> GetTransporterByIdAsync(string id)
    {
        var transporters = await _repository.GetByIdsAsync(new[] { id });
        var transporter = transporters.FirstOrDefault();
        return transporter == null || transporter.IsDeleted ? null : _mapper.Map<TransporterReadDto>(transporter);
    }

    public async Task<TransporterReadDto> CreateTransporterAsync(CreateTransporterDto dto)
    {
        var transporter = _mapper.Map<Transporter>(dto);
        var created = await _repository.CreateAsync(transporter);
        return _mapper.Map<TransporterReadDto>(created);
    }

    public async Task<bool> UpdateTransporterAsync(string id, UpdateTransporterDto dto)
    {
        var transporters = await _repository.GetByIdsAsync(new[] { id });
        var transporter = transporters.FirstOrDefault();
        
        if (transporter == null || transporter.IsDeleted)
        {
            return false;
        }

        _mapper.Map(dto, transporter);
        var updated = await _repository.UpdateAsync(transporter);
        return updated != null;
    }

    public async Task<bool> DeleteTransporterAsync(string id)
    {
        var transporters = await _repository.GetByIdsAsync(new[] { id });
        var transporter = transporters.FirstOrDefault();
        
        if (transporter == null || transporter.IsDeleted)
        {
            return false;
        }

        return await _repository.DeleteAsync(id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var transporters = await _repository.GetByIdsAsync(new[] { id });
        var transporter = transporters.FirstOrDefault();
        return transporter != null && !transporter.IsDeleted;
    }

    public async Task<IEnumerable<TransporterReadDto>> GetTransportersByIdsAsync(IEnumerable<string> ids)
    {
        var uniqueIds = ids.Distinct().ToList();
        var transporters = await _repository.GetByIdsAsync(uniqueIds);
        return _mapper.Map<IEnumerable<TransporterReadDto>>(transporters.Where(t => !t.IsDeleted));
    }
    // Add these methods to TransporterService

    public async Task<IEnumerable<VehicleReadDto>> GetTransporterVehiclesAsync(string transporterId)
    {
        var transporter = (await _repository.GetByIdsAsync(new[] { transporterId })).FirstOrDefault();
        if (transporter == null || transporter.IsDeleted)
        {
            return Enumerable.Empty<VehicleReadDto>();
        }

        // The Transporter.Vehicles navigation property is never populated (no
        // Include/lazy-loading anywhere in this service) — query vehicles by
        // TransporterId directly instead, same pattern as SupplierService.
        var result = await _vehicleRepository.GetPagedAsync(
            pageNumber: 1,
            pageSize: int.MaxValue,
            searchPredicate: v => v.TransporterId == transporterId && !v.IsDeleted);

        return _mapper.Map<IEnumerable<VehicleReadDto>>(result.Items);
    }

    public async Task<IEnumerable<DriverReadDto>> GetTransporterDriversAsync(string transporterId)
    {
        var transporter = (await _repository.GetByIdsAsync(new[] { transporterId })).FirstOrDefault();
        if (transporter == null || transporter.IsDeleted)
        {
            return Enumerable.Empty<DriverReadDto>();
        }

        // Same as above — Transporter.Drivers is never populated, query
        // drivers by TransporterId directly.
        var result = await _driverRepository.GetPagedAsync(
            pageNumber: 1,
            pageSize: int.MaxValue,
            searchPredicate: d => d.TransporterId == transporterId && !d.IsDeleted);

        return _mapper.Map<IEnumerable<DriverReadDto>>(result.Items);
    }
}
