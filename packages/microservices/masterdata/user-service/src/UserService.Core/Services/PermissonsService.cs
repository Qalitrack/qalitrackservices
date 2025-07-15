using AutoMapper;
using UserService.Core.DTOs;
using UserService.Core.DTOs.Permissions;
using UserService.Core.DTOs.Roles;
using UserService.Core.Entities;
using UserService.Core.Interfaces;

namespace UserService.Core.Services;

public class PermissionsService : IPermissionsService
{
    private readonly IPermissionsRepository _permissionsRepository;
    private readonly IMapper _mapper;

    public PermissionsService(IPermissionsRepository permissionsRepository, IMapper mapper)
    {
        _permissionsRepository = permissionsRepository ?? throw new ArgumentNullException(nameof(permissionsRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<IEnumerable<PermissionDto>> GetAllAsync()
    {
        var permissions = await _permissionsRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<PermissionDto>>(permissions);
    }
    

    public async Task<PermissionDto> CreateAsync(DTOs.Permissions.CreatePermissionDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var exists = await _permissionsRepository.DoesPermissionExistAsync(dto.Name);
        if (exists)
            throw new InvalidOperationException($"A permission with name '{dto.Name}' already exists.");

        var permission = _mapper.Map<Permission>(dto);
        var createdPermission = await _permissionsRepository.CreateAsync(permission);
        return _mapper.Map<PermissionDto>(createdPermission);
    }

    public async Task<PermissionDto?> UpdateAsync(string id, DTOs.Permissions.UpdatePermissionDto dto)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentNullException(nameof(id));
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        try
        {
            // 1. First get the existing permission with tracking disabled
            var existingPermission = await _permissionsRepository.GetByIdAsync(id, false); // No tracking
            if (existingPermission == null)
                return null;

            // 2. Check if name is being changed and if new name exists
            if (!string.Equals(existingPermission.Name, dto.Name, StringComparison.OrdinalIgnoreCase))
            {
                var nameExists = await _permissionsRepository.DoesPermissionExistAsync(dto.Name);
                if (nameExists)
                    throw new InvalidOperationException($"A permission with name '{dto.Name}' already exists.");
            }

            // 3. Create a new instance and map properties
            var permissionToUpdate = new Permission
            {
                Id = id,
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = existingPermission.CreatedAt, // Preserve original creation date
                UpdatedAt = DateTime.UtcNow
            };

            // 4. Update the permission
            var updatedPermission = await _permissionsRepository.UpdateAsync(permissionToUpdate);
            return _mapper.Map<PermissionDto>(updatedPermission);
        }
        catch (Exception ex)
        {
            // Log the error
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentNullException(nameof(id));

        return await _permissionsRepository.DeleteAsync(id);
    }

    public async Task<bool> DoesPermissionExistAsync(string name)
    {
        return await _permissionsRepository.DoesPermissionExistAsync(name);
    }

    public async Task<IEnumerable<RoleDto>> GetRolesForPermissionAsync(string permissionId)
    {
        var roles = await _permissionsRepository.GetRolesForPermissionAsync(permissionId);
        return _mapper.Map<IEnumerable<RoleDto>>(roles);
    }

    public async Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId)
    {
        return await _permissionsRepository.AssignPermissionToRoleAsync(roleId, permissionId);
    }

    public async Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId)
    {
        return await _permissionsRepository.RemovePermissionFromRoleAsync(roleId, permissionId);
    }

    public async Task<object?> GetByIdAsync(string id)
    {
        return await _permissionsRepository.GetByIdAsync(id, false);
    }
}
