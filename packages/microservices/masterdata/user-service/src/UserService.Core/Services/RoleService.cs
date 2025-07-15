using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using UserService.Core.DTOs;
using UserService.Core.DTOs.Permissions;
using UserService.Core.DTOs.Role;
using UserService.Core.DTOs.Roles;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserBasicInfoDto = UserService.Core.DTOs.Role.UserBasicInfoDto;

namespace UserService.Core.Services;

public class RoleService(
    IRoleRepository roleRepository,
    IPermissionsRepository permissionRepository,
    IRolePermissionRepository rolePermissionRepository,
    IUserRepository userRepository,
    IMapper mapper,
    ILogger<RoleService> logger)
    : IRoleService
{
    private readonly IRoleRepository _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
    private readonly IPermissionsRepository _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
    private readonly IRolePermissionRepository _rolePermissionRepository = rolePermissionRepository ?? throw new ArgumentNullException(nameof(rolePermissionRepository));
    private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
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

        var roleDto = _mapper.Map<RoleWithUsersDto>(role);
        
        // Get users with this role
        var users = (await _userRepository.GetUsersByRoleAsync(id)).Cast<User>();
        roleDto.Users = users.Select<User, UserBasicInfoDto>(u => new UserBasicInfoDto
        {
            Id = u.Id,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName
        }).ToList();
        
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
    if (roleWithSameName != null && roleWithSameName.Id != id)
    {
        throw new InvalidOperationException("A role with this name already exists.");
    }

    // Explicitly set the ID before mapping
    existingRole.Id = id;
    _mapper.Map(dto, existingRole);
    await _roleRepository.UpdateAsync(existingRole);
    await _roleRepository.SaveChangesAsync();

    return _mapper.Map<RoleDto>(existingRole);
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

    // Check if role is assigned to any users
    if (role.UserRoles != null && role.UserRoles.Any())
    {
        throw new InvalidOperationException("Cannot delete role that is assigned to users.");
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

public async Task<bool> DoesRoleExistAsync(string roleName)
{
    if (string.IsNullOrEmpty(roleName))
        throw new ArgumentException("Role name is required", nameof(roleName));

    var role = await _roleRepository.GetByNameAsync(roleName);
    return role != null;
}

public async Task<IEnumerable<PermissionDto>> GetPermissionsForRoleAsync(string roleId)
{
    if (string.IsNullOrEmpty(roleId))
        throw new ArgumentException("Role ID is required", nameof(roleId));

    try
    {
        _logger.LogInformation("Retrieving permissions for role ID: {RoleId}", roleId);

        // Check if role exists
        var role = await _roleRepository.GetByIdAsync(roleId,true);
        if (role == null)
        {
            _logger.LogWarning("Role with ID {RoleId} not found", roleId);
            throw new KeyNotFoundException($"Role with ID {roleId} not found");
        }

        // Get permissions for the role
        var permissions = await _roleRepository.GetPermissionsForRoleAsync(roleId);
        
        _logger.LogInformation("Successfully retrieved {Count} permissions for role ID: {RoleId}", 
            permissions?.Count() ?? 0, roleId);

        return _mapper.Map<IEnumerable<PermissionDto>>(permissions ?? Enumerable.Empty<Permission>());
    }
    catch (KeyNotFoundException)
    {
        // Re-throw KeyNotFoundException as it's a valid business case
        throw;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving permissions for role ID: {RoleId}", roleId);
        throw new ApplicationException("An error occurred while retrieving role permissions. Please try again later.", ex);
    }
}

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