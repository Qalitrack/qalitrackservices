using Masterdata.Core.DTOs.Organisation;
using Masterdata.Core.DTOs.Affiliation;
using Masterdata.Core.Models;
namespace Masterdata.Core.Interfaces;
public interface IOrganisationService
{
    // Basic CRUD operations
    Task<PagedResult<OrganisationDto>> GetPagedOrganisationsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null);
    Task<OrganisationDto?> GetByIdAsync(Guid id);
    Task<OrganisationDto> CreateAsync(CreateOrganisationDto createDto);
    Task<OrganisationDto?> UpdateAsync(Guid id, UpdateOrganisationDto updateDto);
    Task<bool> DeleteAsync(Guid id);
    
    // Organisation status management
    Task<bool> DeactivateAsync(Guid id);
    Task<bool> ActivateAsync(Guid id);
    
    // Affiliation management
    Task<PagedResult<AffiliationDto>> GetAffiliatesAsync(Guid organisationId, int pageNumber = 1, int pageSize = 10);
    Task<AffiliationDto?> GetAffiliationByIdAsync(Guid affiliationId);
    Task<AffiliationDto> AddAffiliationAsync(Guid organisationId, CreateAffiliationDto createDto);
    Task<bool> RemoveAffiliationAsync(Guid affiliationId);
    
    // Validation and checks
    Task<bool> ExistsAsync(Guid id);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null);
    
    // Bulk operations
    Task<int> ImportOrganisationsAsync(IEnumerable<CreateOrganisationDto> organisationDtos);
    Task<bool> UpdateAffiliationStatusAsync(Guid organisationId, string status);
}
