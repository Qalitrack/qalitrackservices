using AutoMapper;
using System.Linq;
using UserService.Core.DTOs.User;
using UserService.Core.Entities;

namespace UserService.Core.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        // Map from User to UserReadDto
        CreateMap<User, UserReadDto>()
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role)));
            
        // Map from CreateUserDto to User
        CreateMap<CreateUserDto, User>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Email));
            
        // Map from UpdateUserDto to User
        CreateMap<UpdateUserDto, User>();
    }
}