using AutoMapper;
using UserService.Core.DTOs;
using UserService.Core.Entities;

namespace UserService.Core.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles))
            .ForMember(dest => dest.Organizations, opt => opt.MapFrom(src => src.OrganizationUsers));
        
        CreateMap<CreateUserDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => UserStatus.Active))
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
            .ForMember(dest => dest.Sessions, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationUsers, opt => opt.Ignore())
            .ForMember(dest => dest.Profile, opt => opt.Ignore());

        CreateMap<UpdateUserDto, User>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // UserProfile mappings
        CreateMap<UserService.Core.Entities.UserProfile, UserProfileDto>();
        CreateMap<UpdateUserProfileDto, UserService.Core.Entities.UserProfile>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // Role mappings
        CreateMap<Role, RoleDto>()
            .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.RolePermissions.Select(rp => rp.Permission)));
        
        CreateMap<CreateRoleDto, Role>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
            .ForMember(dest => dest.RolePermissions, opt => opt.Ignore());

        CreateMap<UpdateRoleDto, Role>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<UserRole, UserRoleDto>();

        // Permission mappings
        CreateMap<Permission, PermissionDto>();
        CreateMap<CreatePermissionDto, Permission>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.RolePermissions, opt => opt.Ignore());

        CreateMap<UpdatePermissionDto, Permission>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // Organization mappings
        CreateMap<OrganizationUser, OrganizationUserDto>();
        CreateMap<CreateOrganizationUserDto, OrganizationUser>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.JoinedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => OrganizationUserStatus.Active))
            .ForMember(dest => dest.User, opt => opt.Ignore());

        CreateMap<UpdateOrganizationUserDto, OrganizationUser>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // UserInvitation mappings
        CreateMap<UserInvitation, UserInvitationDto>();
        CreateMap<CreateUserInvitationDto, UserInvitation>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Token, opt => opt.Ignore())
            .ForMember(dest => dest.ExpiresAt, opt => opt.MapFrom(src => DateTime.UtcNow.AddDays(7)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => InvitationStatus.Pending));

        // UserSession mappings
        CreateMap<UserSession, UserSessionDto>();

        // Register mappings
        CreateMap<RegisterRequestDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => UserStatus.Pending))
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
            .ForMember(dest => dest.Sessions, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationUsers, opt => opt.Ignore())
            .ForMember(dest => dest.Profile, opt => opt.Ignore());
    }
}