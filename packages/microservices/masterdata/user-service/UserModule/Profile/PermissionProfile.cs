using AutoMapper;
using UserModule.Models;
using UserModule.Dtos;
using UserModule.Dtos.Permissions;
using UserModule.Dtos.Roles;

namespace UserModule.Profiles;

public class PermissionProfile : AutoMapper.Profile
{
    public PermissionProfile()
    {
        CreateMap<Permission, PermissionReadDto>();
        CreateMap<PermissionCreateDto, Permission>();
        CreateMap<PermissionUpdateDto, Permission>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        
        CreateMap<Role, RoleReadDto>()
            .ForMember(dest => dest.Permissions, 
                opt => opt.MapFrom(src => src.RolePermissions.Select(rp => rp.Permission)));
        CreateMap<RoleCreateDto, Role>();
        CreateMap<RoleUpdateDto, Role>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}