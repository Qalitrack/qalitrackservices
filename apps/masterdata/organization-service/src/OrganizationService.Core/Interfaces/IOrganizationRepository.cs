using OrganizationService.Core.Entities;

namespace OrganizationService.Core.Interfaces;

public interface IOrganizationRepository : IRepository<Organization>
{
    Task<Organization?> GetByCodeAsync(string code);
    Task<List<Organization>> GetChildOrganizationsAsync(string parentId);
    Task<List<Organization>> GetOrganizationHierarchyAsync(string organizationId);
    Task<bool> IsCodeUniqueAsync(string code, string? excludeId = null);
    Task<List<Organization>> GetByStatusAsync(OrganizationStatus status);
    Task<int> GetUserCountAsync(string organizationId);
}

public interface IOrganizationUserRepository : IRepository<OrganizationUser>
{
    Task<List<OrganizationUser>> GetByOrganizationIdAsync(string organizationId);
    Task<OrganizationUser?> GetByEmailAsync(string email);
    Task<OrganizationUser?> GetByUserIdAsync(string userId);
    Task<List<OrganizationUser>> GetByRoleAsync(string organizationId, OrganizationRole role);
    Task<bool> IsEmailUniqueInOrganizationAsync(string organizationId, string email, string? excludeId = null);
}

public interface IOrganizationSettingsRepository : IRepository<OrganizationSettings>
{
    Task<OrganizationSettings?> GetByOrganizationIdAsync(string organizationId);
}

public interface IOrganizationDepartmentRepository : IRepository<OrganizationDepartment>
{
    Task<List<OrganizationDepartment>> GetByOrganizationIdAsync(string organizationId);
    Task<OrganizationDepartment?> GetByCodeAsync(string organizationId, string code);
    Task<List<OrganizationDepartment>> GetChildDepartmentsAsync(string parentDepartmentId);
}

public interface IOrganizationLocationRepository : IRepository<OrganizationLocation>
{
    Task<List<OrganizationLocation>> GetByOrganizationIdAsync(string organizationId);
    Task<OrganizationLocation?> GetByCodeAsync(string organizationId, string code);
    Task<OrganizationLocation?> GetHeadquartersAsync(string organizationId);
}