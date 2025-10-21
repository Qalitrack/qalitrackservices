using Masterdata.Core.DTOs.Sacco;
using Masterdata.Core.DTOs.Affiliation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Masterdata.Core.Models;

namespace Masterdata.Core.Interfaces;

public interface ISaccoService
{
    // Basic CRUD operations
    Task<PagedResult<SaccoDto>> GetPagedSaccosAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null);
    Task<SaccoDto?> GetByIdAsync(Guid id);
    Task<SaccoDto> CreateAsync(CreateSaccoDto createDto);
    Task<SaccoDto?> UpdateAsync(Guid id, UpdateSaccoDto updateDto);
    Task<bool> DeleteAsync(Guid id);
    
    // Sacco status management
    Task<bool> DeactivateAsync(Guid id);
    Task<bool> ActivateAsync(Guid id);
    
    // Affiliation management
    Task<PagedResult<AffiliationDto>> GetAffiliatesAsync(Guid saccoId, int pageNumber = 1, int pageSize = 10);
    Task<AffiliationDto?> GetAffiliationByIdAsync(Guid affiliationId);
    Task<AffiliationDto> AddAffiliationAsync(Guid saccoId, CreateAffiliationDto createDto);
    Task<bool> RemoveAffiliationAsync(Guid affiliationId);
    
    // Validation and checks
    Task<bool> ExistsAsync(Guid id);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null);
    
    // Bulk operations
    Task<int> ImportSaccosAsync(IEnumerable<CreateSaccoDto> saccoDtos);
    Task<bool> UpdateAffiliationStatusAsync(Guid saccoId, string status);
}
