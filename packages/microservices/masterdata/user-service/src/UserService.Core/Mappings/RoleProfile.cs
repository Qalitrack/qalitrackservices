using AutoMapper;
using UserService.Core.DTOs.Roles;
using UserService.Core.Entities;

namespace UserService.Core.Mappings;

public class RoleProfile : Profile
{
    public RoleProfile()
    {
        CreateMap<Role, RoleDto>();  // Map Role to RoleDto
        CreateMap<CreateRoleDto, Role>();  // Map CreateRoleDto to Role
        CreateMap<UpdateRoleDto, Role>();  // Map UpdateRoleDto to Role
    }
}