using AutoMapper;
using UserService.Core.DTOs.Roles;
using UserService.Core.DTOs.UserRole;
using UserService.Core.Entities;

namespace UserService.Core.Mappings;

public class UserRoleProfile : Profile
{
    public UserRoleProfile()
    {
        // Map to the detailed UserRoleDto with audit fields
        CreateMap<UserRole, UserService.Core.DTOs.UserRole.UserRoleDto>()
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
            .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.UpdatedBy))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

        // Map to the simpler UserRoleDto in Roles namespace
        CreateMap<UserRole, UserService.Core.DTOs.Roles.UserRoleDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : string.Empty))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty));

        CreateMap<CreateUserRoleDto, UserRole>();
        CreateMap<UpdateUserRoleDto, UserRole>();
    }
}