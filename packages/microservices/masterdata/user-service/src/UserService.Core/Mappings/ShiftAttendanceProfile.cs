using AutoMapper;
using UserService.Core.DTOs.Shift;
using UserService.Core.Entities;
using UserService.Core.Enums;

namespace UserService.Core.Mappings;

 public class ShiftAttendanceProfile : Profile
    {
        public ShiftAttendanceProfile()
        {
            CreateMap<ShiftAttendance, ShiftAttendanceResponse>()
                .ForMember(dest => dest.EmployeeName,
                    opt => opt.MapFrom(src => $"{src.Employee.FirstName} {src.Employee.LastName}"))
                .ForMember(dest => dest.EmployeeEmail,
                    opt => opt.MapFrom(src => src.Employee.Email))
                // ShiftInstance is only eager-loaded on some query paths (e.g. GetAllAsync) —
                // GetByInstanceIdAsync's projection omits it since the caller already knows
                // the shift/instance, so these must null-check rather than assume it's loaded.
                .ForMember(dest => dest.ShiftId,
                    opt => opt.MapFrom(src => src.ShiftInstance != null ? src.ShiftInstance.ShiftId : null))
                .ForMember(dest => dest.ShiftName,
                    opt => opt.MapFrom(src => src.ShiftInstance != null && src.ShiftInstance.Shift != null ? src.ShiftInstance.Shift.Name : null));

            CreateMap<ClockInRequest, ShiftAttendance>()
                .ForMember(dest => dest.ClockInTime, 
                    opt => opt.MapFrom(src => src.ClockInTime ?? DateTime.UtcNow))
                .ForMember(dest => dest.Status, 
                    opt => opt.MapFrom(src => AttendanceStatus.Present))
                .ForMember(dest => dest.Id, 
                    opt => opt.Ignore())
                .ForMember(dest => dest.ClockOutTime, 
                    opt => opt.Ignore())
                .ForMember(dest => dest.IsLate, 
                    opt => opt.Ignore())
                .ForMember(dest => dest.IsEarlyDeparture, 
                    opt => opt.Ignore())
                .ForMember(dest => dest.ActualHoursWorked, 
                    opt => opt.Ignore())
                .ForMember(dest => dest.ShiftInstance, 
                    opt => opt.Ignore())
                .ForMember(dest => dest.Employee, 
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
            

            CreateMap<ShiftAttendance, ClockOutResult>()
                .ForMember(dest => dest.Success, 
                    opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Message, 
                    opt => opt.MapFrom(src => "Clock out successful"))
                .ForMember(dest => dest.Attendance, 
                    opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.TotalHoursWorked, 
                    opt => opt.MapFrom(src => src.ActualHoursWorked));
        }
    }