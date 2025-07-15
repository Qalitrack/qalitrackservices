using AutoMapper;
using UserService.Core.DTOs.RolePermission;
using UserService.Core.Entities;

namespace UserService.Core.Mappings;

public class RolePermissionProfile : Profile
{
    public RolePermissionProfile()
    {
        CreateMap<RolePermission, RolePermissionDto>();
    }
}