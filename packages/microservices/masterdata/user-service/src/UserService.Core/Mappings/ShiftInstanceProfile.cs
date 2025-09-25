using AutoMapper;
using UserService.Core.DTOs.Shift;
using UserService.Core.Entities;
using UserService.Core.Enums;

namespace UserService.Core.Mappings;

  public class ShiftInstanceProfile : Profile
    {
        public ShiftInstanceProfile()
        {
            CreateMap<ShiftInstance, ShiftInstanceResponse>()
                .ForMember(dest => dest.ShiftName,
                    opt => opt.MapFrom(src => src.Shift.Name));
         
            CreateMap<Shift, ShiftInstance>()
                .ForMember(dest => dest.ShiftId, 
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Id, 
                    opt => opt.Ignore())
                .ForMember(dest => dest.ScheduledDate, 
                    opt => opt.Ignore())
                .ForMember(dest => dest.ScheduledStartTime, 
                    opt => opt.Ignore())
                .ForMember(dest => dest.ScheduledEndTime, 
                    opt => opt.Ignore())
                .ForMember(dest => dest.Status, 
                    opt => opt.MapFrom(src => ShiftInstanceStatus.Scheduled))
                .ForMember(dest => dest.Notes, 
                    opt => opt.Ignore())
                .ForMember(dest => dest.Shift, 
                    opt => opt.Ignore())
                .ForMember(dest => dest.Attendances, 
                    opt => opt.Ignore())
                .ForMember(dest => dest.Notifications, 
                    opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, 
                    opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, 
                    opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, 
                    opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, 
                    opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, 
                    opt => opt.Ignore());
        }
    }
