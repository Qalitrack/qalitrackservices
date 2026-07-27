using AutoMapper;
using System.Linq;
using UserService.Core.DTOs.Permissions;
using UserService.Core.Entities;

namespace UserService.Core.Mappings;

public class PermissionProfile : Profile
{
    public PermissionProfile()
    {
        // Map from Permission to PermissionDto
        CreateMap<Permission, PermissionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name ?? string.Empty))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
            .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.UpdatedBy))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsSystem, opt => opt.MapFrom(src => src.IsSystem));
            
        // Map from CreatePermissionDto to Permission
        CreateMap<CreatePermissionDto, Permission>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
            
        // Map from UpdatePermissionDto to Permission
        CreateMap<UpdatePermissionDto, Permission>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
            
        // Map for Permission with Roles
        CreateMap<Permission, PermissionWithRolesDto>()
            .IncludeBase<Permission, PermissionDto>()
            .ForMember(dest => dest.Roles, 
                     opt => opt.MapFrom(src => src.RolePermissions.Select(rp => rp.Role)));
    }
}