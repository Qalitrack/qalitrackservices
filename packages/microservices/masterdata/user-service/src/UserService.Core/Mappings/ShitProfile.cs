using AutoMapper;
using UserService.Core.DTOs.Shift;
using UserService.Core.Entities;

namespace UserService.Core.Mappings;

public class ShitProfile : Profile
{
    public ShitProfile()
    {
        CreateMap<Shift, ShiftDto>();
        CreateMap<CreateShiftDto, Shift>();
        CreateMap<UpdateShiftDto, Shift>();
    }
    
}