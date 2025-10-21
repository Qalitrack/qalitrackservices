using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Masterdata.Core.DTOs;
using Masterdata.Core.DTOs.Weighbridge;
using Masterdata.Core.Entities;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;
using Microsoft.Extensions.Logging;

namespace Masterdata.Core.Services;

public class WeighbridgeService : IWeighbridgeService
{
    private readonly IRepository<Weighbridge> _weighbridgeRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<WeighbridgeService> _logger;

    public WeighbridgeService(
        IRepository<Weighbridge> weighbridgeRepository,
        IMapper mapper,
        ILogger<WeighbridgeService> logger)
    {
        _weighbridgeRepository = weighbridgeRepository ?? throw new ArgumentNullException(nameof(weighbridgeRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<WeighbridgeDto>> GetPagedWeighbridgesAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
    {
        try
        {
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var pagedResult = await _weighbridgeRepository.GetPagedAsync(
                pageNumber: pageNumber,
                pageSize: pageSize,
                searchTerm: searchTerm,
                searchProperties: new[] { nameof(Weighbridge.Location), nameof(Weighbridge.Description) }
            );

            if (!pagedResult.Items.Any())
            {
                return new PagedResult<WeighbridgeDto>
                {
                    Items = Enumerable.Empty<WeighbridgeDto>(),
                    TotalItems = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }

            var items = pagedResult.Items.Select(w => _mapper.Map<WeighbridgeDto>(w)).ToList();

            return new PagedResult<WeighbridgeDto>
            {
                Items = items,
                TotalItems = pagedResult.TotalItems,
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged weighbridges");
            throw;
        }
    }

    public async Task<WeighbridgeDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var weighbridges = await _weighbridgeRepository.GetByIdsAsync(new[] { id.ToString() });
            var weighbridge = weighbridges.FirstOrDefault();
            return weighbridge != null ? _mapper.Map<WeighbridgeDto>(weighbridge) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving weighbridge with ID: {id}");
            throw;
        }
    }

    public async Task<WeighbridgeDto> CreateAsync(CreateWeighbridgeDto dto)
    {
        try
        {
            if (await IsLocationAvailableAsync(dto.Location) == false)
            {
                throw new InvalidOperationException($"A weighbridge at location '{dto.Location}' already exists.");
            }

            var weighbridge = _mapper.Map<Weighbridge>(dto);
            var createdWeighbridge = await _weighbridgeRepository.CreateAsync(weighbridge);
            return _mapper.Map<WeighbridgeDto>(createdWeighbridge);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating weighbridge");
            throw;
        }
    }

    public async Task<WeighbridgeDto?> UpdateAsync(Guid id, UpdateWeighbridgeDto dto)
    {
        try
        {
            var weighbridges = await _weighbridgeRepository.GetByIdsAsync(new[] { id.ToString() });
            var existingWeighbridge = weighbridges.FirstOrDefault();
            if (existingWeighbridge == null)
            {
                return null;
            }

            if (await IsLocationAvailableAsync(dto.Location, id) == false)
            {
                throw new InvalidOperationException($"A weighbridge at location '{dto.Location}' already exists.");
            }

            _mapper.Map(dto, existingWeighbridge);
            var updatedWeighbridge = await _weighbridgeRepository.UpdateAsync(existingWeighbridge);
            return updatedWeighbridge != null ? _mapper.Map<WeighbridgeDto>(updatedWeighbridge) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating weighbridge with ID: {id}");
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            return await _weighbridgeRepository.DeleteAsync(id.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting weighbridge with ID: {id}");
            throw;
        }
    }

    public async Task<bool> IsLocationAvailableAsync(string location, Guid? excludeId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                return false;
            }

            var result = await _weighbridgeRepository.GetPagedAsync(
                pageNumber: 1,
                pageSize: 1,
                searchTerm: location,
                searchProperties: new[] { nameof(Weighbridge.Location) }
            );

            if (!result.Items.Any())
            {
                return true;
            }

            if (excludeId.HasValue)
            {
                return result.Items.All(w => w.Id == excludeId.Value.ToString());
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking weighbridge location availability for: {location}");
            throw;
        }
    }
}
