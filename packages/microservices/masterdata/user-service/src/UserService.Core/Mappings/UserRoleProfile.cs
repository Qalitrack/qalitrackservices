using AutoMapper;
using UserService.Core.Entities;
using UserService.Core.DTOs;
using UserService.Core.DTOs.Roles;

public class UserRoleProfile : Profile
{
    public UserRoleProfile()
    {
        CreateMap<UserRole, UserRoleDto>();
        CreateMap<CreateUserRoleDto, UserRole>();
        CreateMap<UpdateUserRoleDto, UserRole>();
    }
}