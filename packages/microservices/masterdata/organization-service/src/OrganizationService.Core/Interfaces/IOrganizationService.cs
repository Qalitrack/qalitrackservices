using OrganizationService.Core.DTOs;
using OrganizationService.Core.Entities;

namespace OrganizationService.Core.Interfaces;

public interface IOrganizationService
{
    Task<OrganizationDto> CreateOrganizationAsync(CreateOrganizationRequest request);
    Task<OrganizationDto> UpdateOrganizationAsync(string id, UpdateOrganizationRequest request);
    Task<OrganizationDto?> GetOrganizationByIdAsync(string id);
    Task<OrganizationDto?> GetOrganizationByCodeAsync(string code);
    Task<List<OrganizationDto>> GetAllOrganizationsAsync();
    Task<List<OrganizationDto>> GetChildOrganizationsAsync(string parentId);
    Task<List<OrganizationDto>> GetOrganizationHierarchyAsync(string organizationId);
    Task DeleteOrganizationAsync(string id);
    Task<bool> ValidateOrganizationCodeAsync(string code, string? excludeId = null);
    
    // User Management
    Task<OrganizationUserDto> CreateOrganizationUserAsync(string organizationId, CreateOrganizationUserRequest request);
    Task<OrganizationUserDto> UpdateOrganizationUserAsync(string organizationId, string userId, UpdateOrganizationUserRequest request);
    Task<List<OrganizationUserDto>> GetOrganizationUsersAsync(string organizationId);
    Task<OrganizationUserDto?> GetOrganizationUserByIdAsync(string organizationId, string userId);
    Task DeleteOrganizationUserAsync(string organizationId, string userId);
    Task<OrganizationUserDto> InviteUserAsync(string organizationId, string email, OrganizationRole role);
    
    // Settings Management
    Task<OrganizationSettingsDto?> GetOrganizationSettingsAsync(string organizationId);
    Task<OrganizationSettingsDto> UpdateOrganizationSettingsAsync(string organizationId, UpdateOrganizationSettingsRequest request);
    Task<OrganizationSettingsDto> CreateDefaultSettingsAsync(string organizationId);
}