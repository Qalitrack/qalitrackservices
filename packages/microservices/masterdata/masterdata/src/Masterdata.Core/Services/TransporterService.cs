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
    private readonly IMapper _mapper;
    private readonly ILogger<TransporterService> _logger;

    public TransporterService(
        IRepository<Transporter> repository,
        IMapper mapper,
        ILogger<TransporterService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<TransporterReadDto>> GetPagedTransportersAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null)
    {
        var pagedResult = await _repository.GetPagedAsync(
            pageNumber,
            pageSize,
            searchTerm,
            searchTerm != null 
                ? t => t.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                       t.ContactInfo != null && t.ContactInfo.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                : null,
            new[] { nameof(Transporter.Name), nameof(Transporter.ContactInfo) }
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
        var transporters = await _repository.GetByIdsAsync(new[] { transporterId });
        var transporter = transporters.FirstOrDefault();
    
        if (transporter == null || transporter.IsDeleted)
        {
            return Enumerable.Empty<VehicleReadDto>();
        }

        // Assuming the Transporter entity has a navigation property for Vehicles
        if (transporter.Vehicles == null || !transporter.Vehicles.Any())
        {
            return Enumerable.Empty<VehicleReadDto>();
        }

        return _mapper.Map<IEnumerable<VehicleReadDto>>(transporter.Vehicles.Where(v => !v.IsDeleted));
    }

    public async Task<IEnumerable<DriverReadDto>> GetTransporterDriversAsync(string transporterId)
    {
        var transporters = await _repository.GetByIdsAsync(new[] { transporterId });
        var transporter = transporters.FirstOrDefault();
    
        if (transporter == null || transporter.IsDeleted)
        {
            return Enumerable.Empty<DriverReadDto>();
        }

        // Assuming the Transporter entity has a navigation property for Drivers
        if (transporter.Drivers == null || !transporter.Drivers.Any())
        {
            return Enumerable.Empty<DriverReadDto>();
        }

        return _mapper.Map<IEnumerable<DriverReadDto>>(transporter.Drivers.Where(d => !d.IsDeleted));
    }
}
