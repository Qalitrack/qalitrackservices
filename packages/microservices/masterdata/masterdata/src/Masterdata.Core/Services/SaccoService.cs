using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Masterdata.Core.DTOs.Sacco;
using Masterdata.Core.DTOs.Affiliation;
using Masterdata.Core.Entities;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;
using Microsoft.Extensions.Logging;

namespace Masterdata.Core.Services;

public class SaccoService : ISaccoService
{
    private readonly IRepository<Sacco> _saccoRepository;
    private readonly IRepository<Affiliation> _affiliationRepository;
    private readonly IRepository<Organisation> _organisationRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<SaccoService> _logger;

    public SaccoService(
        IRepository<Sacco> saccoRepository,
        IRepository<Affiliation> affiliationRepository,
        IRepository<Organisation> organisationRepository,
        IMapper mapper,
        ILogger<SaccoService> logger)
    {
        _saccoRepository = saccoRepository ?? throw new ArgumentNullException(nameof(saccoRepository));
        _affiliationRepository = affiliationRepository ?? throw new ArgumentNullException(nameof(affiliationRepository));
        _organisationRepository = organisationRepository ?? throw new ArgumentNullException(nameof(organisationRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<SaccoDto>> GetPagedSaccosAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
    {
        try
        {
            var result = await _saccoRepository.GetPagedAsync(
                pageNumber,
                pageSize,
                searchTerm,
                string.IsNullOrEmpty(searchTerm) 
                    ? null 
                    : s => s.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

            return new PagedResult<SaccoDto>
            {
                Items = _mapper.Map<IEnumerable<SaccoDto>>(result.Items),
                TotalItems = result.TotalItems,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paginated saccos");
            throw;
        }
    }

    public async Task<SaccoDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var saccos = await _saccoRepository.GetByIdsAsync(new[] { id.ToString() });
            var sacco = saccos.FirstOrDefault();
            return sacco != null ? _mapper.Map<SaccoDto>(sacco) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sacco with ID: {SaccoId}", id);
            throw;
        }
    }

    public async Task<SaccoDto> CreateAsync(CreateSaccoDto createDto)
    {
        try
        {
            if (await NameExistsAsync(createDto.Name))
            {
                throw new InvalidOperationException($"A sacco with name '{createDto.Name}' already exists.");
            }

            var sacco = _mapper.Map<Sacco>(createDto);
            var created = await _saccoRepository.CreateAsync(sacco);
            return _mapper.Map<SaccoDto>(created);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Error creating sacco");
            throw;
        }
    }

    public async Task<SaccoDto?> UpdateAsync(Guid id, UpdateSaccoDto updateDto)
    {
        try
        {
            var saccos = await _saccoRepository.GetByIdsAsync(new[] { id.ToString() });
            var existingSacco = saccos.FirstOrDefault();
            if (existingSacco == null)
            {
                return null;
            }

            if (await NameExistsAsync(updateDto.Name, id))
            {
                throw new InvalidOperationException($"A sacco with name '{updateDto.Name}' already exists.");
            }

            _mapper.Map(updateDto, existingSacco);
            await _saccoRepository.UpdateAsync(existingSacco);
            
            return _mapper.Map<SaccoDto>(existingSacco);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Error updating sacco with ID: {SaccoId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var saccos = await _saccoRepository.GetByIdsAsync(new[] { id.ToString() });
            var sacco = saccos.FirstOrDefault();
            if (sacco == null)
            {
                return false;
            }

            // Check for active affiliations
            var activeAffiliations = await _affiliationRepository.GetPagedAsync(
                pageNumber: 1,
                pageSize: 1,
                searchPredicate: a => a.SaccoId == id.ToString()
            );

            if (activeAffiliations.TotalItems > 0)
            {
                throw new InvalidOperationException("Cannot delete sacco with active affiliations.");
            }

            return await _saccoRepository.DeleteAsync(id.ToString());
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Error deleting sacco with ID: {SaccoId}", id);
            throw;
        }
    }

    public async Task<bool> DeactivateAsync(Guid id)
    {
        try
        {
            var saccos = await _saccoRepository.GetByIdsAsync(new[] { id.ToString() });
            var sacco = saccos.FirstOrDefault();
            if (sacco == null)
            {
                return false;
            }

            sacco.Status = "Inactive";
            await _saccoRepository.UpdateAsync(sacco);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating sacco with ID: {SaccoId}", id);
            throw;
        }
    }

    public async Task<bool> ActivateAsync(Guid id)
    {
        try
        {
            var saccos = await _saccoRepository.GetByIdsAsync(new[] { id.ToString() });
            var sacco = saccos.FirstOrDefault();
            if (sacco == null)
            {
                return false;
            }

            sacco.Status = "Active";
            await _saccoRepository.UpdateAsync(sacco);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating sacco with ID: {SaccoId}", id);
            throw;
        }
    }

    public async Task<PagedResult<AffiliationDto>> GetAffiliatesAsync(Guid saccoId, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var result = await _affiliationRepository.GetPagedAsync(
                pageNumber,
                pageSize,
                searchPredicate: a => a.SaccoId == saccoId.ToString());

            // Get all related data in separate queries
            var organisationIds = result.Items
                .Select(a => a.OrganisationId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();
                
            var organisations = (await _organisationRepository.GetByIdsAsync(organisationIds))
                .ToDictionary(o => o.Id, o => o);
            
            var saccoIds = result.Items
                .Select(a => a.SaccoId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();
                
            var saccos = (await _saccoRepository.GetByIdsAsync(saccoIds))
                .ToDictionary(s => s.Id, s => s);
            
            // Map to DTOs with related data
            var affiliationDtos = new List<AffiliationDto>();
            foreach (var affiliation in result.Items)
            {
                var dto = _mapper.Map<AffiliationDto>(affiliation);
                
                if (affiliation.OrganisationId != null && organisations.TryGetValue(affiliation.OrganisationId, out var org))
                {
                    dto.OrganisationName = org.Name;
                }
                
                if (affiliation.SaccoId != null && saccos.TryGetValue(affiliation.SaccoId, out var sacco))
                {
                    dto.SaccoName = sacco.Name;
                }
                
                affiliationDtos.Add(dto);
            }

            return new PagedResult<AffiliationDto>
            {
                Items = affiliationDtos,
                TotalItems = result.TotalItems,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving affiliates for sacco ID: {SaccoId}", saccoId);
            throw;
        }
    }

    public async Task<AffiliationDto?> GetAffiliationByIdAsync(Guid affiliationId)
    {
        try
        {
            var affiliations = await _affiliationRepository.GetByIdsAsync(new[] { affiliationId.ToString() });
            var affiliation = affiliations.FirstOrDefault();
            if (affiliation == null)
            {
                return null;
            }

            var dto = _mapper.Map<AffiliationDto>(affiliation);
            
            // Load related data if needed
            if (!string.IsNullOrEmpty(affiliation.OrganisationId))
            {
                var orgs = await _organisationRepository.GetByIdsAsync(new[] { affiliation.OrganisationId });
                var org = orgs.FirstOrDefault();
                if (org != null)
                {
                    dto.OrganisationName = org.Name;
                }
            }
            
            if (!string.IsNullOrEmpty(affiliation.SaccoId))
            {
                var saccos = await _saccoRepository.GetByIdsAsync(new[] { affiliation.SaccoId });
                var sacco = saccos.FirstOrDefault();
                if (sacco != null)
                {
                    dto.SaccoName = sacco.Name;
                }
            }

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving affiliation with ID: {AffiliationId}", affiliationId);
            throw;
        }
    }

    public async Task<AffiliationDto> AddAffiliationAsync(Guid saccoId, CreateAffiliationDto createDto)
{
    try
    {
        // Check if sacco exists
        var saccos = await _saccoRepository.GetByIdsAsync(new[] { saccoId.ToString() });
        if (!saccos.Any())
        {
            throw new KeyNotFoundException($"Sacco with ID {saccoId} not found.");
        }

        // Check if organisation exists
        if (!Guid.TryParse(createDto.OrganisationId, out var orgId))
        {
            throw new ArgumentException("Invalid Organisation ID format");
        }
        
        var orgs = await _organisationRepository.GetByIdsAsync(new[] { orgId.ToString() });
        if (!orgs.Any())
        {
            throw new KeyNotFoundException($"Organisation with ID {createDto.OrganisationId} not found.");
        }

        // Check if affiliation already exists
        var existing = await _affiliationRepository.GetPagedAsync(
            pageNumber: 1,
            pageSize: 1,
            searchPredicate: a => a.SaccoId == saccoId.ToString() && 
                                a.OrganisationId == orgId.ToString()
        );

        if (existing.TotalItems > 0)
        {
            throw new InvalidOperationException("An affiliation between this sacco and organisation already exists.");
        }

        var affiliation = _mapper.Map<Affiliation>(createDto);
        affiliation.SaccoId = saccoId.ToString();
        // Ensure we're using the parsed orgId to avoid any potential format issues
        affiliation.OrganisationId = orgId.ToString();
        
        var created = await _affiliationRepository.CreateAsync(affiliation);
        
        // Convert the created ID to Guid for the GetAffiliationByIdAsync call
        if (!Guid.TryParse(created.Id, out var createdId))
        {
            throw new InvalidOperationException("Invalid ID format for the created affiliation");
        }
        
        return await GetAffiliationByIdAsync(createdId);
    }
    catch (Exception ex) when (ex is not KeyNotFoundException and not InvalidOperationException)
    {
        _logger.LogError(ex, "Error adding affiliation to sacco ID: {SaccoId}", saccoId);
        throw;
    }
}

    public async Task<bool> RemoveAffiliationAsync(Guid affiliationId)
    {
        try
        {
            return await _affiliationRepository.DeleteAsync(affiliationId.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing affiliation with ID: {AffiliationId}", affiliationId);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var saccos = await _saccoRepository.GetByIdsAsync(new[] { id.ToString() });
        return saccos.Any();
    }

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var normalizedName = name.Trim().ToLower();

        if (excludeId.HasValue)
        {
            var excludeIdStr = excludeId.Value.ToString();
            return await _saccoRepository.ExistsByPredicateAsync(
                s => s.Name.ToLower() == normalizedName && s.Id != excludeIdStr);
        }

        return await _saccoRepository.ExistsByPredicateAsync(
            s => s.Name.ToLower() == normalizedName);
    }

    public async Task<int> ImportSaccosAsync(IEnumerable<CreateSaccoDto> saccoDtos)
    {
        int importedCount = 0;
        
        foreach (var dto in saccoDtos)
        {
            try
            {
                if (!await NameExistsAsync(dto.Name))
                {
                    await CreateAsync(dto);
                    importedCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing sacco with name: {SaccoName}", dto.Name);
                // Continue with the next sacco
            }
        }
        
        return importedCount;
    }

    public async Task<bool> UpdateAffiliationStatusAsync(Guid saccoId, string status)
    {
        try
        {
            var affiliations = await _affiliationRepository.GetPagedAsync(
                pageNumber: 1,
                pageSize: int.MaxValue,
                searchPredicate: a => a.SaccoId == saccoId.ToString()
            );

            if (!affiliations.Items.Any())
            {
                return false;
            }

            foreach (var affiliation in affiliations.Items)
            {
                affiliation.Status = status;
                await _affiliationRepository.UpdateAsync(affiliation);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating affiliation status for sacco ID: {SaccoId}", saccoId);
            throw;
        }
    }
}
