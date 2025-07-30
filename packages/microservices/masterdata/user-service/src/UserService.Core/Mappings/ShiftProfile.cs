// In ShiftProfile.cs
using AutoMapper;
using UserService.Core.DTOs.Shift;
using UserService.Core.Entities;

namespace UserService.Core.Mappings
{
    public class ShiftProfile : Profile
    {
        public ShiftProfile()
        {
            // Map from Shift to ShiftDto
            CreateMap<Shift, ShiftDto>()
                .ForMember(dest => dest.StartTime, 
                    opt => opt.MapFrom(src => DateTime.Today.Add(src.StartTime)))
                .ForMember(dest => dest.EndTime, 
                    opt => opt.MapFrom(src => DateTime.Today.Add(src.EndTime)))
                .ForMember(dest => dest.IsActive, 
                    opt => opt.MapFrom(src => src.IsActive)) // This will use the computed property
                .ForMember(dest => dest.AutoRepeatDaily, 
                    opt => opt.MapFrom(src => src.AutoRepeatDaily))
                .ForMember(dest => dest.DurationMinutes, 
                    opt => opt.MapFrom(src => src.DurationMinutes));

            // Map from CreateShiftDto to Shift
            CreateMap<CreateShiftDto, Shift>()
                .ForMember(dest => dest.StartTime, 
                    opt => opt.MapFrom(src => src.StartTime.TimeOfDay))
                .ForMember(dest => dest.EndTime, 
                    opt => opt.Ignore()) // EndTime will be calculated in service
                .ForMember(dest => dest.Mode, 
                    opt => opt.MapFrom(src => src.Mode)) // Explicitly map Mode
                .ForMember(dest => dest.AutoRepeatDaily, 
                    opt => opt.MapFrom(src => src.AutoRepeatDaily))
                .ForMember(dest => dest.DurationMinutes, 
                    opt => opt.MapFrom(src => src.DurationMinutes))
                .ForMember(dest => dest.IsActive, 
                    opt => opt.Ignore()); // Ignore computed property

            // Map from UpdateShiftDto to Shift
            CreateMap<UpdateShiftDto, Shift>()
                .ForMember(dest => dest.StartTime, 
                    opt => opt.MapFrom(src => src.StartTime.TimeOfDay))
                .ForMember(dest => dest.EndTime, 
                    opt => opt.Ignore()) // EndTime will be calculated in service
                .ForMember(dest => dest.Mode, 
                    opt => opt.MapFrom(src => src.Mode)) // Explicitly map Mode
                .ForMember(dest => dest.AutoRepeatDaily, 
                    opt => opt.MapFrom(src => src.AutoRepeatDaily))
                .ForMember(dest => dest.DurationMinutes, 
                    opt => opt.MapFrom(src => src.DurationMinutes))
                .ForMember(dest => dest.IsActive, 
                    opt => opt.Ignore()); // Ignore computed property
        }
    }
}