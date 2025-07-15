using AutoMapper;
using System.Linq;
using UserService.Core.DTOs;
using UserService.Core.DTOs.Roles;
using UserService.Core.Entities;

namespace UserService.Core.Mappings;

public class RoleProfile : Profile
{
    public RoleProfile()
    {
        // Map from Role to RoleDto
        CreateMap<Role, RoleDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name ?? string.Empty))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
            
        // Map from CreateRoleDto to Role
        CreateMap<CreateRoleDto, Role>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
            
        // Map from UpdateRoleDto to Role
        CreateMap<UpdateRoleDto, Role>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
            
        // Map for Role with Permissions
        CreateMap<Role, RoleWithPermissionsDto>()
            .IncludeBase<Role, RoleDto>()
            .ForMember(dest => dest.Permissions, 
                      opt => opt.MapFrom(src => src.RolePermissions.Select(rp => rp.Permission)));

        // Map from User to UserBasicInfoDto
        CreateMap<User, UserBasicInfoDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email ?? string.Empty))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName ?? string.Empty))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName ?? string.Empty));
    }
}