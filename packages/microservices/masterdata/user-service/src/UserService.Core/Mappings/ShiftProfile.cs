using AutoMapper;
using UserService.Core.DTOs.Shift;
using UserService.Core.Entities;
using UserService.Core.Enums;

namespace UserService.Core.Mappings
{
    public class ShiftProfile : Profile
    {
        public ShiftProfile()
        {
            // Map from Shift to ShiftDto - Add AssignedUsers count
            CreateMap<Shift, ShiftDto>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.AssignedUsers, opt => opt.MapFrom(src => 
                    src.UserShifts != null ? src.UserShifts
                        .Count(us => !us.IsDeleted && us.User != null && !us.User.IsDeleted) : 0));

            // Map from Shift to ShiftResponse
            CreateMap<Shift, ShiftResponse>()
                .ForMember(dest => dest.CustomDays, opt => opt.MapFrom(src => src.CustomDays))
                .ForMember(dest => dest.ExceptionDates, opt => opt.MapFrom(src => src.ExceptionDates))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.AssignedUsers, opt => opt.MapFrom(src => 
                    src.UserShifts != null ? src.UserShifts
                        .Count(us => !us.IsDeleted && us.User != null && !us.User.IsDeleted) : 0));

            // Map from CreateShiftRequest to Shift
            CreateMap<CreateShiftRequest, Shift>()
                .ForMember(dest => dest.CustomDays, opt => opt.MapFrom(src => src.CustomDays))
                .ForMember(dest => dest.ExceptionDates, opt => opt.MapFrom(src => src.ExceptionDates))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => ShiftStatus.Active))
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());

            // Map from UpdateShiftRequest to Shift
            CreateMap<UpdateShiftRequest, Shift>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());
                
            // Map from UserShift to AssignedUserDto
            CreateMap<UserShift, Core.DTOs.Report.AssignedUserDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.AssignedAt, opt => opt.MapFrom(src => src.AssignedAt))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => !src.User.IsDeleted));
        }
    }
}