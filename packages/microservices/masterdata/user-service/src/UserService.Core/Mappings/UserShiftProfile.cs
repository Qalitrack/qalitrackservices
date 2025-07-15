using AutoMapper;
using UserService.Core.DTOs;
using UserService.Core.Entities;

namespace UserService.Core.Mappings;

public class UserShiftProfile : Profile
{
    public UserShiftProfile()
    {
        CreateMap<UserShift, UserShiftDto>();
    }
}