using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Masterdata.Core.DTOs.Affiliation;
using Masterdata.Core.DTOs.Organisation;
using Masterdata.Core.Entities;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;
using Microsoft.Extensions.Logging;

namespace Masterdata.Core.Services;

public class OrganisationService : IOrganisationService
{
    private readonly IRepository<Organisation> _organisationRepository;
    private readonly IRepository<Affiliation> _affiliationRepository;
    private readonly IRepository<Sacco> _saccoRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<OrganisationService> _logger;

    public OrganisationService(
        IRepository<Organisation> organisationRepository,
        IRepository<Affiliation> affiliationRepository,
        IRepository<Sacco> saccoRepository,
        IMapper mapper,
        ILogger<OrganisationService> logger)
    {
        _organisationRepository = organisationRepository ?? throw new ArgumentNullException(nameof(organisationRepository));
        _affiliationRepository = affiliationRepository ?? throw new ArgumentNullException(nameof(affiliationRepository));
        _saccoRepository = saccoRepository ?? throw new ArgumentNullException(nameof(saccoRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<OrganisationDto>> GetPagedOrganisationsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
    {
        try
        {
            var result = await _organisationRepository.GetPagedAsync(
                pageNumber,
                pageSize,
                searchTerm,
                searchProperties: new[] { nameof(Organisation.Name), nameof(Organisation.Type) });

            return new PagedResult<OrganisationDto>
            {
                Items = _mapper.Map<IEnumerable<OrganisationDto>>(result.Items),
                TotalItems = result.TotalItems,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged organisations. Page: {PageNumber}, PageSize: {PageSize}", pageNumber, pageSize);
            throw;
        }
    }

    public async Task<OrganisationDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var organisations = await _organisationRepository.GetByIdsAsync(new[] { id.ToString() });
            var organisation = organisations.FirstOrDefault();
            return organisation != null ? _mapper.Map<OrganisationDto>(organisation) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving organisation with ID: {OrganisationId}", id);
            throw;
        }
    }

    public async Task<OrganisationDto> CreateAsync(CreateOrganisationDto createDto)
    {
        if (createDto == null) throw new ArgumentNullException(nameof(createDto));

        try
        {
            if (await NameExistsAsync(createDto.Name))
            {
                throw new InvalidOperationException($"An organisation with name '{createDto.Name}' already exists.");
            }

            var organisation = _mapper.Map<Organisation>(createDto);
            organisation.Status = "active";
            
            var created = await _organisationRepository.CreateAsync(organisation);
            return _mapper.Map<OrganisationDto>(created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating organisation with name: {OrganisationName}", createDto.Name);
            throw;
        }
    }

    public async Task<OrganisationDto?> UpdateAsync(Guid id, UpdateOrganisationDto updateDto)
    {
        if (updateDto == null) throw new ArgumentNullException(nameof(updateDto));

        try
        {
            var organisations = await _organisationRepository.GetByIdsAsync(new[] { id.ToString() });
            var organisation = organisations.FirstOrDefault();
            if (organisation == null) return null;

            if (await NameExistsAsync(updateDto.Name, id))
            {
                throw new InvalidOperationException($"Another organisation with name '{updateDto.Name}' already exists.");
            }

            _mapper.Map(updateDto, organisation);
            await _organisationRepository.UpdateAsync(organisation);
            
            return _mapper.Map<OrganisationDto>(organisation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating organisation with ID: {OrganisationId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            // Check if organisation exists
            var organisations = await _organisationRepository.GetByIdsAsync(new[] { id.ToString() });
            var organisation = organisations.FirstOrDefault();
            if (organisation == null) return false;

            // Check if there are any active affiliations
            var activeAffiliations = await _affiliationRepository.GetPagedAsync(
                pageNumber: 1,
                pageSize: 1, // We only need to know if there are any
                searchPredicate: a => a.OrganisationId == id.ToString()
            );
            var hasActiveAffiliations = activeAffiliations.TotalItems > 0;
            if (hasActiveAffiliations)
            {
                throw new InvalidOperationException("Cannot delete organisation with active affiliations. Remove affiliations first.");
            }

            // Soft delete by setting status to inactive
            organisation.Status = "inactive";
            await _organisationRepository.UpdateAsync(organisation);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting organisation with ID: {OrganisationId}", id);
            throw;
        }
    }

    public async Task<bool> DeactivateAsync(Guid id)
    {
        try
        {
            var organisations = await _organisationRepository.GetByIdsAsync(new[] { id.ToString() });
            var organisation = organisations.FirstOrDefault();
            if (organisation == null) return false;

            organisation.Status = "inactive";
            await _organisationRepository.UpdateAsync(organisation);
            
            // Optionally deactivate all affiliations
            await UpdateAffiliationStatusAsync(id, "inactive");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating organisation with ID: {OrganisationId}", id);
            throw;
        }
    }

    public async Task<bool> ActivateAsync(Guid id)
    {
        try
        {
            var organisations = await _organisationRepository.GetByIdsAsync(new[] { id.ToString() });
            var organisation = organisations.FirstOrDefault();
            if (organisation == null) return false;

            organisation.Status = "active";
            await _organisationRepository.UpdateAsync(organisation);
            
            // Optionally activate all affiliations
            await UpdateAffiliationStatusAsync(id, "active");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating organisation with ID: {OrganisationId}", id);
            throw;
        }
    }

    public async Task<PagedResult<AffiliationDto>> GetAffiliatesAsync(Guid organisationId, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            // First get the paginated affiliations
            var result = await _affiliationRepository.GetPagedAsync(
                pageNumber,
                pageSize,
                searchPredicate: a => a.OrganisationId == organisationId.ToString());

            // Get all related data in separate queries
            var affiliationDtos = new List<AffiliationDto>();
            
            // Get all organization IDs
            var organizationIds = result.Items
                .Select(a => a.OrganisationId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();
                
            // Get all organizations
            var organizations = (await _organisationRepository.GetByIdsAsync(organizationIds))
                .ToDictionary(o => o.Id, o => o);
                
            // Get all sacco IDs
            var saccoIds = result.Items
                .Select(a => a.SaccoId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();
                
            // Get all saccos
            var saccos = (await _saccoRepository.GetByIdsAsync(saccoIds))
                .ToDictionary(s => s.Id, s => s);
            
            // Map to DTOs with related data
            foreach (var affiliation in result.Items)
            {
                var dto = _mapper.Map<AffiliationDto>(affiliation);
                
                // Set organization name if available
                if (affiliation.OrganisationId != null && organizations.TryGetValue(affiliation.OrganisationId, out var org))
                {
                    dto.OrganisationName = org.Name;
                }
                
                // Set sacco name if available
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
            _logger.LogError(ex, "Error retrieving affiliates for organisation ID: {OrganisationId}", organisationId);
            throw;
        }
    }

    public async Task<AffiliationDto?> GetAffiliationByIdAsync(Guid affiliationId)
    {
        try
        {
            var affiliations = await _affiliationRepository.GetByIdsAsync(new[] { affiliationId.ToString() });
            var affiliation = affiliations.FirstOrDefault();
            return affiliation != null ? _mapper.Map<AffiliationDto>(affiliation) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving affiliation with ID: {AffiliationId}", affiliationId);
            throw;
        }
    }

    public async Task<AffiliationDto> AddAffiliationAsync(Guid organisationId, CreateAffiliationDto createDto)
    {
        if (createDto == null) throw new ArgumentNullException(nameof(createDto));

        try
        {
            // Check if organisation exists
            var organisations = await _organisationRepository.GetByIdsAsync(new[] { organisationId.ToString() });
            var organisation = organisations.FirstOrDefault();
            if (organisation == null)
            {
                throw new KeyNotFoundException($"Organisation with ID {organisationId} not found.");
            }

            // Check if sacco exists if provided
            if (!string.IsNullOrEmpty(createDto.SaccoId) && 
                !(await _saccoRepository.GetByIdsAsync(new[] { createDto.SaccoId })).Any())
            {
                throw new KeyNotFoundException($"Sacco with ID {createDto.SaccoId} not found.");
            }

            var affiliation = _mapper.Map<Affiliation>(createDto);
            affiliation.OrganisationId = organisationId.ToString();
            
            var created = await _affiliationRepository.CreateAsync(affiliation);
            return _mapper.Map<AffiliationDto>(created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding affiliation for organisation ID: {OrganisationId}", organisationId);
            throw;
        }
    }

    public async Task<bool> RemoveAffiliationAsync(Guid affiliationId)
    {
        try
        {
            await _affiliationRepository.DeleteAsync(affiliationId.ToString());
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing affiliation with ID: {AffiliationId}", affiliationId);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        try
        {
            var organisations = await _organisationRepository.GetByIdsAsync(new[] { id.ToString() });
            return organisations.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if organisation exists with ID: {OrganisationId}", id);
            throw;
        }
    }

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null)
    {
        try
        {
            var existing = await _organisationRepository.GetPagedAsync(
                pageNumber: 1,
                pageSize: 1,
                searchTerm: name.Trim(),
                searchPredicate: o => 
                    o.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    (excludeId == null || o.Id != excludeId.ToString()));

            return existing.TotalItems > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if organisation name exists: {OrganisationName}", name);
            throw;
        }
    }

    public async Task<int> ImportOrganisationsAsync(IEnumerable<CreateOrganisationDto> organisationDtos)
    {
        if (organisationDtos == null) throw new ArgumentNullException(nameof(organisationDtos));

        int importedCount = 0;
        
        foreach (var dto in organisationDtos)
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
                _logger.LogError(ex, "Error importing organisation with name: {OrganisationName}", dto.Name);
                // Continue with the next organisation
            }
        }
        
        return importedCount;
    }

    public async Task<bool> UpdateAffiliationStatusAsync(Guid organisationId, string status)
    {
        if (string.IsNullOrWhiteSpace(status)) 
            throw new ArgumentException("Status cannot be empty", nameof(status));

        try
        {
            // Get all affiliations for the organisation
            var affiliations = await _affiliationRepository.GetPagedAsync(
                pageNumber: 1,
                pageSize: int.MaxValue,
                searchPredicate: a => a.OrganisationId == organisationId.ToString()
            );

            // Update status for each affiliation
            foreach (var affiliation in affiliations.Items)
            {
                // Assuming there's a Status property on Affiliation
                // If not, you might need to adjust this part
                var property = typeof(Affiliation).GetProperty("Status");
                if (property != null && property.CanWrite)
                {
                    property.SetValue(affiliation, status);
                    await _affiliationRepository.UpdateAsync(affiliation);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating affiliation status for organisation ID: {OrganisationId}", organisationId);
            throw;
        };
    }
}
