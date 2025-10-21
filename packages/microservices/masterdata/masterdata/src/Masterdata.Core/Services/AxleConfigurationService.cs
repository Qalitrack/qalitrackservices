using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Masterdata.Core.DTOs.Axle;
using Masterdata.Core.Entities;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;
using Microsoft.Extensions.Logging;

namespace Masterdata.Core.Services;

public class AxleConfigurationService : IAxleConfigurationService
{
    private readonly IRepository<AxleConfiguration> _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<AxleConfigurationService> _logger;

    public AxleConfigurationService(
        IRepository<AxleConfiguration> repository,
        IMapper mapper,
        ILogger<AxleConfigurationService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AxleConfigurationDto> GetAxleConfigurationByIdAsync(string id)
    {
        try
        {
            _logger.LogInformation("Getting axle configuration with ID: {Id}", id);
            
            var result = await _repository.GetPagedAsync(
                pageNumber: 1,
                pageSize: 1,
                searchPredicate: x => x.Id == id);

            var axleConfig = result.Items.FirstOrDefault();
            if (axleConfig == null)
            {
                _logger.LogWarning("Axle configuration with ID {Id} not found", id);
                return null;
            }

            return _mapper.Map<AxleConfigurationDto>(axleConfig);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting axle configuration with ID: {Id}", id);
            throw;
        }
    }

    public async Task<PagedResult<AxleConfigurationDto>> GetAxleConfigurationsAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Getting axle configurations - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
            
            var result = await _repository.GetPagedAsync(
                pageNumber: pageNumber,
                pageSize: pageSize,
                sortBy: "CreatedAt",
                sortDescending: true);

            return new PagedResult<AxleConfigurationDto>
            {
                Items = _mapper.Map<List<AxleConfigurationDto>>(result.Items),
                TotalItems = result.TotalItems,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting axle configurations");
            throw;
        }
    }

    public async Task<AxleConfigurationDto> CreateAxleConfigurationAsync(CreateAxleConfigurationDto dto)
    {
        try
        {
            _logger.LogInformation("Creating new axle configuration with code: {Code}", dto.Code);
            
            // Check if code already exists using GetPagedAsync
            var existing = await _repository.GetPagedAsync(
                pageNumber: 1,
                pageSize: 1,
                searchPredicate: x => x.Code == dto.Code);
                
            if (existing.Items.Any())
            {
                throw new InvalidOperationException($"An axle configuration with code '{dto.Code}' already exists.");
            }

            var axleConfig = _mapper.Map<AxleConfiguration>(dto);
            axleConfig.Id = Guid.NewGuid().ToString();
            axleConfig.CreatedAt = DateTime.UtcNow;
            axleConfig.IsActive = true;

            var created = await _repository.CreateAsync(axleConfig);
            return _mapper.Map<AxleConfigurationDto>(created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating axle configuration with code: {Code}", dto.Code);
            throw;
        }
    }

    public async Task<AxleConfigurationDto> UpdateAxleConfigurationAsync(UpdateAxleConfigurationDto dto)
    {
        try
        {
            _logger.LogInformation("Updating axle configuration with ID: {Id}", dto.Id);
            
            // Get existing configuration
            var existingResult = await _repository.GetPagedAsync(
                pageNumber: 1,
                pageSize: 1,
                searchPredicate: x => x.Id == dto.Id);
                
            var existing = existingResult.Items.FirstOrDefault();
            if (existing == null)
            {
                _logger.LogWarning("Axle configuration with ID {Id} not found for update", dto.Id);
                return null;
            }

            // Check if code is being changed and if the new code already exists
            if (!string.IsNullOrEmpty(dto.Code) && dto.Code != existing.Code)
            {
                var codeExistsResult = await _repository.GetPagedAsync(
                    pageNumber: 1,
                    pageSize: 1,
                    searchPredicate: x => x.Code == dto.Code && x.Id != dto.Id);
                    
                if (codeExistsResult.Items.Any())
                {
                    throw new InvalidOperationException($"An axle configuration with code '{dto.Code}' already exists.");
                }
                existing.Code = dto.Code;
            }

            // Update only the properties that were provided in the DTO
            if (!string.IsNullOrEmpty(dto.Description)) existing.Description = dto.Description;
            if (dto.AxleCount.HasValue) existing.AxleCount = dto.AxleCount.Value;
            if (dto.MaxLoadCapacity.HasValue) existing.MaxLoadCapacity = dto.MaxLoadCapacity.Value;
            if (dto.IsActive.HasValue) existing.IsActive = dto.IsActive.Value;
            
            existing.UpdatedAt = DateTime.UtcNow;

            var updated = await _repository.UpdateAsync(existing);
            if (updated == null)
            {
                _logger.LogWarning("Failed to update axle configuration with ID: {Id}", dto.Id);
                return null;
            }

            return _mapper.Map<AxleConfigurationDto>(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating axle configuration with ID: {Id}", dto.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAxleConfigurationAsync(string id)
    {
        try
        {
            _logger.LogInformation("Deleting axle configuration with ID: {Id}", id);
            
            // Check if exists first
            var exists = await _repository.ExistsAsync(id);
            if (!exists)
            {
                _logger.LogWarning("Axle configuration with ID {Id} not found for deletion", id);
                return false;
            }

            return await _repository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting axle configuration with ID: {Id}", id);
            throw;
        }
    }

    public async Task<bool> ToggleAxleConfigurationStatusAsync(string id, bool isActive)
    {
        try
        {
            _logger.LogInformation("Toggling status for axle configuration with ID: {Id} to {Status}", id, isActive ? "Active" : "Inactive");
            
            var result = await _repository.GetPagedAsync(
                pageNumber: 1,
                pageSize: 1,
                searchPredicate: x => x.Id == id);
                
            var axleConfig = result.Items.FirstOrDefault();
            if (axleConfig == null)
            {
                _logger.LogWarning("Axle configuration with ID {Id} not found for status update", id);
                return false;
            }

            if (axleConfig.IsActive == isActive)
            {
                return true; // No change needed
            }

            axleConfig.IsActive = isActive;
            axleConfig.UpdatedAt = DateTime.UtcNow;

            var updated = await _repository.UpdateAsync(axleConfig);
            return updated != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling status for axle configuration with ID: {Id}", id);
            throw;
        }
    }
}
