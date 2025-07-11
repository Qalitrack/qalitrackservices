using AutoMapper;
using UserService.Core.DTOs.Permissions;
using UserService.Core.Entities;

namespace UserService.Core.Mappings;

public class PermissionProfile : Profile
{
    public PermissionProfile()
    {
        CreateMap<Permission, PermissionDto>();
        CreateMap<CreatePermissionDto, Permission>();
        CreateMap<UpdatePermissionDto, Permission>();
    }
}