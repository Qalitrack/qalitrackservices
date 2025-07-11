using AutoMapper;
using UserService.Core.DTOs;
using UserService.Core.DTOs.User;
using UserService.Core.Entities;

namespace UserService.Core.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserReadDto>();
        CreateMap<CreateUserDto, User>();
        CreateMap<UpdateUserDto, User>();
    }
}