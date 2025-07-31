using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using UserService.Core.DTOs;
using UserService.Core.DTOs.Permissions;
using UserService.Core.DTOs.Roles;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
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
    _logger.LogInformation("Checking for duplicate role name '{Name}'. Found role: {FoundRole}, Current ID: {CurrentId}", 
        dto.Name, roleWithSameName?.Id, id);
    
    if (roleWithSameName != null && roleWithSameName.Id != id)
    {
        _logger.LogWarning("Role name '{Name}' already exists for role ID {ExistingId}, cannot update role {CurrentId}", 
            dto.Name, roleWithSameName.Id, id);
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
        _logger.LogInformation("Deactivating role {RoleId}. Checking for user assignments.", id);
        
        // Get all users assigned to this role
        var usersWithRole = (await _userRepository.GetUsersByRoleAsync(id)).Cast<User>();
        if (usersWithRole != null && usersWithRole.Any())
        {
            var userCount = usersWithRole.Count();
            _logger.LogInformation("Removing {UserCount} user assignments from deactivated role {RoleId}", userCount, id);
            
            // Remove all user-role assignments
            foreach (var user in usersWithRole)
            {
                var removed = await _userRoleRepository.RemoveRoleFromUserAsync(user.Id, id);
                if (removed)
                {
                    _logger.LogInformation("Successfully removed role {RoleId} from user {UserId}", id, user.Id);
                }
                else
                {
                    _logger.LogWarning("Failed to remove role {RoleId} from user {UserId}", id, user.Id);
                }
            }
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

    // Log if role is assigned to users - cascade delete will handle cleanup
    if (role.UserRoles != null && role.UserRoles.Cast<UserRole>().Any())
    {
        var userCount = role.UserRoles.Cast<UserRole>().Count();
        _logger.LogInformation("Deleting role {RoleId} which is assigned to {UserCount} users. User-role assignments will be removed automatically.", id, userCount);
    }

    // Log if role has permissions - cascade delete will handle cleanup
    if (role.RolePermissions != null && role.RolePermissions.Cast<RolePermission>().Any())
    {
        var permissionCount = role.RolePermissions.Cast<RolePermission>().Count();
        _logger.LogInformation("Deleting role {RoleId} which has {PermissionCount} permissions. Role-permission assignments will be removed automatically.", id, permissionCount);
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
    var role = await _roleRepository.GetByIdAsync(roleId,true);
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
    var permission = await _permissionRepository.GetByIdAsync(permissionId,true);
    if (permission == null)
    {
        throw new KeyNotFoundException("Permission not found.");
    }

    // Check if the role already has this permission
    var existingRolePermission = await _rolePermissionRepository.GetByRoleAndPermissionAsync(roleId, permissionId);
    if (existingRolePermission != null)
    {
        return true; // Already assigned
    }

    // Assign the permission
    var rolePermission = new RolePermission
    {
        RoleId = roleId,
        PermissionId = permissionId,
        AssignedAt = DateTime.UtcNow
    };

    await _rolePermissionRepository.AddAsync(rolePermission);
    await _rolePermissionRepository.SaveChangesAsync();
    return true;
}

public async Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId)
{
    if (string.IsNullOrEmpty(roleId))
        throw new ArgumentException("Role ID is required", nameof(roleId));
    if (string.IsNullOrEmpty(permissionId))
        throw new ArgumentException("Permission ID is required", nameof(permissionId));

    // Check if role exists and is active
    var role = await _roleRepository.GetByIdAsync(roleId, true);
    if (role == null)
    {
        throw new KeyNotFoundException("Role not found.");
    }

    if (!role.IsActive)
    {
        throw new InvalidOperationException("Cannot remove permissions from an inactive role.");
    }

    // Get the role-permission relationship
    var rolePermission = await _rolePermissionRepository.GetByRoleAndPermissionAsync(roleId, permissionId);
    if (rolePermission == null)
    {
        return false; // Not assigned, nothing to remove
    }

    // Remove the permission
    await _rolePermissionRepository.DeleteAsync(rolePermission);
    await _rolePermissionRepository.SaveChangesAsync();
    return true;
}

/*public async Task<bool> DoesRoleExistAsync(string roleName)
{
    if (string.IsNullOrEmpty(roleName))
        throw new ArgumentException("Role name is required", nameof(roleName));

    var role = await _roleRepository.GetByNameAsync(roleName);
    return role != null;
}*/

// public async Task<IEnumerable<PermissionDto>> GetPermissionsForRoleAsync(string roleId)
// {
//     if (string.IsNullOrEmpty(roleId))
//         throw new ArgumentException("Role ID is required", nameof(roleId));
//
//     try
//     {
//         _logger.LogInformation("Retrieving permissions for role ID: {RoleId}", roleId);
//
//         // Check if role exists
//         var role = await _roleRepository.GetByIdAsync(roleId,true);
//         if (role == null)
//         {
//             _logger.LogWarning("Role with ID {RoleId} not found", roleId);
//             throw new KeyNotFoundException($"Role with ID {roleId} not found");
//         }
//
//         // Get permissions for the role
//         var permissions = await _roleRepository.GetPermissionsForRoleAsync(roleId);
//         
//         _logger.LogInformation("Successfully retrieved {Count} permissions for role ID: {RoleId}", 
//             permissions?.Count() ?? 0, roleId);
//
//         return _mapper.Map<IEnumerable<PermissionDto>>(permissions ?? Enumerable.Empty<Permission>());
//     }
//     catch (KeyNotFoundException)
//     {
//         // Re-throw KeyNotFoundException as it's a valid business case
//         throw;
//     }
//     catch (Exception ex)
//     {
//         _logger.LogError(ex, "Error retrieving permissions for role ID: {RoleId}", roleId);
//         throw new ApplicationException("An error occurred while retrieving role permissions. Please try again later.", ex);
//     }
// }

    public async Task<IEnumerable<Role>> GetRolesByUserIdAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentException("User ID is required", nameof(userId));

        try
        {
            _logger.LogInformation("Retrieving roles for user ID: {UserId}", userId);

            var userRoles = await _userRepository.GetUserRolesAsync(userId);
            if (userRoles == null)
            {
                _logger.LogWarning("No roles found for user ID {UserId}", userId);
                return Enumerable.Empty<Role>();
            }

            var roles = await _roleRepository.GetByIdsAsync(userRoles.Select(ur => ur.RoleId));
            _logger.LogInformation("Successfully retrieved {Count} roles for user ID: {UserId}", 
                roles?.Count() ?? 0, userId);

            return roles ?? Enumerable.Empty<Role>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles for user ID: {UserId}", userId);
            throw new ApplicationException("An error occurred while retrieving user roles. Please try again later.", ex);
        }
    }
}