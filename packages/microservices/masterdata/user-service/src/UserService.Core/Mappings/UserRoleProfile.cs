using AutoMapper;
using UserService.Core.DTOs.Roles;
using UserService.Core.Entities;

namespace UserService.Core.Mappings;

public class UserRoleProfile : Profile
{
    public UserRoleProfile()
    {
        CreateMap<UserRole, UserRoleDto>();
        CreateMap<CreateUserRoleDto, UserRole>();
        CreateMap<UpdateUserRoleDto, UserRole>();
    }
}