using AutoMapper;
using UserModule.Controllers;
using UserModule.Dtos;
using UserModule.Dtos.Users;
using UserModule.Models;

namespace UserModule.Profile;

public class UserProfiles : AutoMapper.Profile
{
    public UserProfiles()
    {
        CreateMap<User, UserCreateDto>();
        CreateMap<UserCreateDto, User>();
    }
}
