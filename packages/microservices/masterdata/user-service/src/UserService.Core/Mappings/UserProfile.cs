using AutoMapper;
using UserService.Core.DTOs;
using UserService.Core.Entities;

namespace UserService.Core.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserService.Core.Entities.User, UserReadDto>();
        CreateMap<CreateUserDto, UserService.Core.Entities.User>();
        CreateMap<UpdateUserDto, UserService.Core.Entities.User>();
    }
}