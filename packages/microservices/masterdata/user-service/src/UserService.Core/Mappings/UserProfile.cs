using AutoMapper;
using System.Linq;
using UserService.Core.DTOs.User;
using UserService.Core.Entities;

namespace UserService.Core.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // Map from User to UserReadDto
            CreateMap<User, UserReadDto>()
                .ForMember(dest => dest.Roles, opt => 
                    opt.MapFrom(src => src.UserRoles
                        .Where(ur => !ur.IsDeleted && ur.Role != null && !ur.Role.IsDeleted)
                        .Select(ur => ur.Role.Name)
                        .ToList()));

            // Map from CreateUserDto to User
            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Email));

            // Add reverse mapping (from UserReadDto to User)
            CreateMap<UserReadDto, User>()
                .ForMember(dest => dest.UserRoles, opt => 
                    opt.MapFrom(src => (src.Roles ?? Enumerable.Empty<string>()).Select(roleName => new UserRole
                    {
                        Role = new Role { Name = roleName }
                    }).ToList()));

            // Map from UpdateUserDto to User
            CreateMap<UpdateUserDto, User>();

            // Another mapping from UserReadDto to User (possibly redundant, but added as per the provided code)
            CreateMap<UserReadDto, User>();
        }
    }
}