using AutoMapper;
using Microsoft.Extensions.Logging;
using UserService.Core.DTOs.Roles;
using UserService.Core.Entities;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.Interfaces.Services;
using UserService.Core.DTOs.Common;
using UserService.Core.DTOs.RolePermission;
using UserBasicInfoDto = UserService.Core.DTOs.Roles.UserBasicInfoDto;

namespace UserService.Core.Services;

public class RoleService(
    IRoleRepository roleRepository,
    IPermissionsRepository permissionRepository,
    IRolePermissionRepository rolePermissionRepository,
    IUserRepository userRepository,
    IUserRoleRepository userRoleRepository,
    IMapper mapper,
    ILogger<RoleService> logger)
    : IRoleService
{
    private readonly IRoleRepository _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
    private readonly IPermissionsRepository _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
    private readonly IRolePermissionRepository _rolePermissionRepository = rolePermissionRepository ?? throw new ArgumentNullException(nameof(rolePermissionRepository));
    private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    private readonly IUserRoleRepository _userRoleRepository = userRoleRepository ?? throw new ArgumentNullException(nameof(userRoleRepository));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ILogger<RoleService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IEnumerable<RoleDto>> GetAllAsync()
    {
        var roles = await _roleRepository.GetAllAsync();
        var result = new List<RoleDto>();

        foreach (var role in roles)
        {
            var roleDto = _mapper.Map<RoleDto>(role);
            
            // Get users with this role
            var users = (await _userRepository.GetUsersByRoleAsync(role.Id)).Cast<User>();
            roleDto.Users = _mapper.Map<List<DTOs.Roles.UserBasicInfoDto>>(users);
            roleDto.TotalUsers = roleDto.Users.Count;
            result.Add(roleDto);
        }

        return result;
    }

    public async Task<RoleDto?> GetByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Role ID is required", nameof(id));

        var role = await _roleRepository.GetByIdAsync(id, true);
        if (role == null)
            return null;

        var roleDto = _mapper.Map<RoleDto>(role);
        
        // Get users with this role
        var users = (await _userRepository.GetUsersByRoleAsync(id)).Cast<User>();
        roleDto.Users = users.Select(u => _mapper.Map<UserBasicInfoDto>(u)).ToList();
        
        roleDto.TotalUsers = roleDto.Users.Count;
        
        return roleDto;
    }

public async Task<RoleDto> CreateAsync(DTOs.Roles.CreateRoleDto dto)
{
    if (dto == null)
        throw new ArgumentNullException(nameof(dto));

    // Check if role with same name exists
    var existingRole = await _roleRepository.GetByNameAsync(dto.Name);
    if (existingRole != null)
    {
        throw new InvalidOperationException("A role with this name already exists.");
    }

    var role = _mapper.Map<Role>(dto);
    role.CreatedAt = DateTime.UtcNow;
    var createdRole = await _roleRepository.AddAsync(role);
    await _roleRepository.SaveChangesAsync();

    return _mapper.Map<RoleDto>(createdRole);
}

public async Task<RoleDto?> UpdateAsync(string id, DTOs.Roles.UpdateRoleDto dto)
{
    if (dto == null)
        throw new ArgumentNullException(nameof(dto));

    var existingRole = await _roleRepository.GetByIdAsync(id, true);
    if (existingRole == null)
    {
        throw new KeyNotFoundException("Role not found.");
    }

    // Check if another role with the same name exists
    var roleWithSameName = await _roleRepository.GetByNameAsync(dto.Name);
    if (roleWithSameName != null && roleWithSameName.Id != id)
    {
        throw new InvalidOperationException("A role with this name already exists.");
    }

    // Check if trying to deactivate a role that has active users assigned
    if (existingRole.IsActive && !dto.IsActive)
    {
        var usersWithRole = await _userRepository.GetUsersByRoleAsync(id);
        if (usersWithRole.Cast<User>().Any())
        {
            throw new InvalidOperationException("Cannot deactivate a role that has active users assigned. Remove all users from this role first.");
        }
    }

    // If deactivating the role, remove all user assignments
    if (existingRole.IsActive && !dto.IsActive)
    {
        
        // Get users count with this role
        var users = await _userRepository.GetUsersByRoleAsync(id);
        var userCount = users.Count();
        if (userCount > 0)
        {
    
            // Use the existing method to remove role from all users
            var removedCount = await _userRoleRepository.RemoveRoleFromAllUsersAsync(id);
            
        }
    }

    // Explicitly set the ID before mapping
    existingRole.Id = id;
    _mapper.Map(dto, existingRole);
    var updatedRole = await _roleRepository.UpdateAsync(existingRole);

    return _mapper.Map<RoleDto>(updatedRole ?? existingRole);
}

public async Task<bool> DeleteAsync(string id)
{
    if (string.IsNullOrEmpty(id))
        throw new ArgumentException("Role ID is required", nameof(id));

    Role? role = await _roleRepository.GetByIdAsync(id,true);
    if (role == null)
    {
        return false;
    }
     
    await _roleRepository.DeleteAsync(id);
    await _roleRepository.SaveChangesAsync();
    return true;
}

public async Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId)
{
    if (string.IsNullOrEmpty(roleId))
        throw new ArgumentException("Role ID is required", nameof(roleId));
    if (string.IsNullOrEmpty(permissionId))
        throw new ArgumentException("Permission ID is required", nameof(permissionId));

    // Check if role exists
    var role = await _roleRepository.GetByIdAsync(roleId, true);
    if (role == null)
    {
        throw new KeyNotFoundException("Role not found.");
    }

    // Check if role is active
    if (!role.IsActive)
    {
        throw new InvalidOperationException("Cannot assign permissions to an inactive role.");
    }

    // Check if permission exists
    var permission = await _permissionRepository.GetByIdAsync(permissionId, true);
    if (permission == null)
    {
        throw new KeyNotFoundException("Permission not found.");
    }

    // Use our new method that properly handles both new assignments and restoring deleted ones
    var success = await _rolePermissionRepository.AssignOrRestorePermissionToRoleAsync(roleId, permissionId);
    
    if (success)
    {
        _logger.LogInformation("Permission {PermissionId} successfully assigned to role {RoleId}", permissionId, roleId);
    }
    else
    {
        _logger.LogWarning("Failed to assign permission {PermissionId} to role {RoleId}", permissionId, roleId);
    }
    
    return success;
}

public async Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId)
{
    if (string.IsNullOrEmpty(roleId))
        throw new ArgumentException("Role ID is required", nameof(roleId));
    
    if (string.IsNullOrEmpty(permissionId))
        throw new ArgumentException("Permission ID is required", nameof(permissionId));

    // Check if role exists - adding the second parameter for includeRelated
    var role = await _roleRepository.GetByIdAsync(roleId, false);
    if (role == null)
    {
        _logger.LogWarning("Cannot remove permission from non-existent role: {RoleId}", roleId);
        return false;
    }

    // Check if permission exists - adding the second parameter for includeRelated
    var permission = await _permissionRepository.GetByIdAsync(permissionId, false);
    if (permission == null)
    {
        _logger.LogWarning("Cannot remove non-existent permission: {PermissionId}", permissionId);
        return false;
    }

    // Use the specialized method to delete by both roleId and permissionId
    var success = await _rolePermissionRepository.DeleteByRoleAndPermissionAsync(roleId, permissionId);
    
    if (success)
    {
        _logger.LogInformation("Removed permission {PermissionId} from role {RoleId}", permissionId, roleId);
    }
    else
    {
        _logger.LogWarning("Permission {PermissionId} was not assigned to role {RoleId}", permissionId, roleId);
    }
    
    return success;
}

public async Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(string roleId)
{
    if (string.IsNullOrEmpty(roleId))
        throw new ArgumentException("Role ID is required", nameof(roleId));

    // Check if role exists
    var role = await _roleRepository.GetByIdAsync(roleId, false);
    if (role == null)
    {
        throw new KeyNotFoundException("Role not found.");
    }

    // Get permissions for the role
    return await _roleRepository.GetPermissionsForRoleAsync(roleId);
}

public async Task<PagedResult<RoleDto>> GetDeletedPagedAsync(PaginationParameters parameters)
{
    if (parameters == null)
        throw new ArgumentNullException(nameof(parameters));

    var pagedRoles = await _roleRepository.GetDeletedPagedAsync(parameters);
    var mappedRoles = _mapper.Map<IEnumerable<RoleDto>>(pagedRoles.Items);

    return new PagedResult<RoleDto>
    {
        Items = mappedRoles,
        Page = pagedRoles.Page,
        PageSize = pagedRoles.PageSize,
        TotalCount = pagedRoles.TotalCount
    };
}
public async Task<PagedResult<RolePermissionDto>> GetDeletedRolePermissionsPagedAsync(PaginationParameters parameters)
{
    if (parameters == null)
        throw new ArgumentNullException(nameof(parameters));

    var pagedRolePermissions = await _rolePermissionRepository.GetDeletedPagedAsync(parameters);
    var mappedRolePermissions = _mapper.Map<IEnumerable<RolePermissionDto>>(pagedRolePermissions.Items);

    return new PagedResult<RolePermissionDto>
    {
        Items = mappedRolePermissions,
        Page = pagedRolePermissions.Page,
        PageSize = pagedRolePermissions.PageSize,
        TotalCount = pagedRolePermissions.TotalCount
    };
}
}