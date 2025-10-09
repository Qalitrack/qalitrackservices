using AutoMapper;
using UserService.Core.DTOs;
using UserService.Core.DTOs.Shift;
using UserService.Core.Entities;

namespace UserService.Core.Mappings;

public class UserShiftProfile : Profile
{
    public UserShiftProfile()
    {
        CreateMap<UserShift, UserShiftDto>()
            .ForMember(dest => dest.ShiftName, opt => opt.MapFrom(src => src.Shift.Name))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.UpdatedBy));
    }
}