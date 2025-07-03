using AutoMapper;
using OrganizationService.Core.DTOs;
using OrganizationService.Core.Entities;
using OrganizationService.Core.Interfaces;

namespace OrganizationService.Core.Services;

public class OrganizationService : IOrganizationService
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IOrganizationUserRepository _organizationUserRepository;
    private readonly IOrganizationSettingsRepository _organizationSettingsRepository;
    private readonly IMapper _mapper;

    public OrganizationService(
        IOrganizationRepository organizationRepository,
        IOrganizationUserRepository organizationUserRepository,
        IOrganizationSettingsRepository organizationSettingsRepository,
        IMapper mapper)
    {
        _organizationRepository = organizationRepository;
        _organizationUserRepository = organizationUserRepository;
        _organizationSettingsRepository = organizationSettingsRepository;
        _mapper = mapper;
    }

    public async Task<OrganizationDto> CreateOrganizationAsync(CreateOrganizationRequest request)
    {
        if (!await ValidateOrganizationCodeAsync(request.Code))
        {
            throw new InvalidOperationException($"Organization code '{request.Code}' already exists.");
        }

        var organization = _mapper.Map<Organization>(request);
        organization.Id = Guid.NewGuid().ToString();
        organization.CreatedAt = DateTime.UtcNow;
        organization.Status = OrganizationStatus.Active;

        var createdOrganization = await _organizationRepository.AddAsync(organization);
        
        // Create default settings
        await CreateDefaultSettingsAsync(createdOrganization.Id);
        
        return _mapper.Map<OrganizationDto>(createdOrganization);
    }

    public async Task<OrganizationDto> UpdateOrganizationAsync(string id, UpdateOrganizationRequest request)
    {
        var organization = await _organizationRepository.GetByIdAsync(id);
        if (organization == null)
        {
            throw new ArgumentException($"Organization with ID '{id}' not found.");
        }

        _mapper.Map(request, organization);
        organization.UpdatedAt = DateTime.UtcNow;

        var updatedOrganization = await _organizationRepository.UpdateAsync(organization);
        return _mapper.Map<OrganizationDto>(updatedOrganization);
    }

    public async Task<OrganizationDto?> GetOrganizationByIdAsync(string id)
    {
        var organization = await _organizationRepository.GetByIdAsync(id);
        return organization != null ? _mapper.Map<OrganizationDto>(organization) : null;
    }

    public async Task<OrganizationDto?> GetOrganizationByCodeAsync(string code)
    {
        var organization = await _organizationRepository.GetByCodeAsync(code);
        return organization != null ? _mapper.Map<OrganizationDto>(organization) : null;
    }

    public async Task<List<OrganizationDto>> GetAllOrganizationsAsync()
    {
        var organizations = await _organizationRepository.GetAllAsync();
        return _mapper.Map<List<OrganizationDto>>(organizations);
    }

    public async Task<List<OrganizationDto>> GetChildOrganizationsAsync(string parentId)
    {
        var children = await _organizationRepository.GetChildOrganizationsAsync(parentId);
        return _mapper.Map<List<OrganizationDto>>(children);
    }

    public async Task<List<OrganizationDto>> GetOrganizationHierarchyAsync(string organizationId)
    {
        var hierarchy = await _organizationRepository.GetOrganizationHierarchyAsync(organizationId);
        return _mapper.Map<List<OrganizationDto>>(hierarchy);
    }

    public async Task DeleteOrganizationAsync(string id)
    {
        var organization = await _organizationRepository.GetByIdAsync(id);
        if (organization == null)
        {
            throw new ArgumentException($"Organization with ID '{id}' not found.");
        }

        // Check if organization has children
        var children = await _organizationRepository.GetChildOrganizationsAsync(id);
        if (children.Any())
        {
            throw new InvalidOperationException("Cannot delete organization with child organizations.");
        }

        // Check if organization has users
        var userCount = await _organizationRepository.GetUserCountAsync(id);
        if (userCount > 0)
        {
            throw new InvalidOperationException("Cannot delete organization with active users.");
        }

        await _organizationRepository.DeleteAsync(id);
    }

    public async Task<bool> ValidateOrganizationCodeAsync(string code, string? excludeId = null)
    {
        return await _organizationRepository.IsCodeUniqueAsync(code, excludeId);
    }

    // User Management
    public async Task<OrganizationUserDto> CreateOrganizationUserAsync(string organizationId, CreateOrganizationUserRequest request)
    {
        var organization = await _organizationRepository.GetByIdAsync(organizationId);
        if (organization == null)
        {
            throw new ArgumentException($"Organization with ID '{organizationId}' not found.");
        }

        if (!await _organizationUserRepository.IsEmailUniqueInOrganizationAsync(organizationId, request.Email))
        {
            throw new InvalidOperationException($"User with email '{request.Email}' already exists in this organization.");
        }

        var organizationUser = _mapper.Map<OrganizationUser>(request);
        organizationUser.Id = Guid.NewGuid().ToString();
        organizationUser.OrganizationId = organizationId;
        organizationUser.UserId = Guid.NewGuid().ToString(); // This should come from User Service
        organizationUser.CreatedAt = DateTime.UtcNow;

        var createdUser = await _organizationUserRepository.AddAsync(organizationUser);
        return _mapper.Map<OrganizationUserDto>(createdUser);
    }

    public async Task<OrganizationUserDto> UpdateOrganizationUserAsync(string organizationId, string userId, UpdateOrganizationUserRequest request)
    {
        var organizationUser = await _organizationUserRepository.FirstOrDefaultAsync(u => u.OrganizationId == organizationId && u.Id == userId);
        if (organizationUser == null)
        {
            throw new ArgumentException($"User with ID '{userId}' not found in organization '{organizationId}'.");
        }

        _mapper.Map(request, organizationUser);
        organizationUser.UpdatedAt = DateTime.UtcNow;

        var updatedUser = await _organizationUserRepository.UpdateAsync(organizationUser);
        return _mapper.Map<OrganizationUserDto>(updatedUser);
    }

    public async Task<List<OrganizationUserDto>> GetOrganizationUsersAsync(string organizationId)
    {
        var users = await _organizationUserRepository.GetByOrganizationIdAsync(organizationId);
        return _mapper.Map<List<OrganizationUserDto>>(users);
    }

    public async Task<OrganizationUserDto?> GetOrganizationUserByIdAsync(string organizationId, string userId)
    {
        var user = await _organizationUserRepository.FirstOrDefaultAsync(u => u.OrganizationId == organizationId && u.Id == userId);
        return user != null ? _mapper.Map<OrganizationUserDto>(user) : null;
    }

    public async Task DeleteOrganizationUserAsync(string organizationId, string userId)
    {
        var organizationUser = await _organizationUserRepository.FirstOrDefaultAsync(u => u.OrganizationId == organizationId && u.Id == userId);
        if (organizationUser == null)
        {
            throw new ArgumentException($"User with ID '{userId}' not found in organization '{organizationId}'.");
        }

        await _organizationUserRepository.DeleteAsync(organizationUser);
    }

    public async Task<OrganizationUserDto> InviteUserAsync(string organizationId, string email, OrganizationRole role)
    {
        var request = new CreateOrganizationUserRequest
        {
            Email = email,
            Role = role
        };

        return await CreateOrganizationUserAsync(organizationId, request);
    }

    // Settings Management
    public async Task<OrganizationSettingsDto?> GetOrganizationSettingsAsync(string organizationId)
    {
        var settings = await _organizationSettingsRepository.GetByOrganizationIdAsync(organizationId);
        return settings != null ? _mapper.Map<OrganizationSettingsDto>(settings) : null;
    }

    public async Task<OrganizationSettingsDto> UpdateOrganizationSettingsAsync(string organizationId, UpdateOrganizationSettingsRequest request)
    {
        var settings = await _organizationSettingsRepository.GetByOrganizationIdAsync(organizationId);
        if (settings == null)
        {
            throw new ArgumentException($"Settings for organization '{organizationId}' not found.");
        }

        _mapper.Map(request, settings);
        settings.UpdatedAt = DateTime.UtcNow;

        var updatedSettings = await _organizationSettingsRepository.UpdateAsync(settings);
        return _mapper.Map<OrganizationSettingsDto>(updatedSettings);
    }

    public async Task<OrganizationSettingsDto> CreateDefaultSettingsAsync(string organizationId)
    {
        var settings = new OrganizationSettings
        {
            Id = Guid.NewGuid().ToString(),
            OrganizationId = organizationId,
            CreatedAt = DateTime.UtcNow
        };

        var createdSettings = await _organizationSettingsRepository.AddAsync(settings);
        return _mapper.Map<OrganizationSettingsDto>(createdSettings);
    }
}