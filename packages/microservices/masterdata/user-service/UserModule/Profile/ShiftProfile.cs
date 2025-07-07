using AutoMapper;
using UserModule.Dtos.Shift;
using UserModule.Dtos.Users;
using UserModule.Models;
using ShiftCreateDto = UserModule.Dtos.Shift.ShiftCreateDto;

namespace UserModule.Profile
{
    public class ShiftProfile : AutoMapper.Profile
    {
        public ShiftProfile()
        {
            // Shift mappings
            CreateMap<Shift, ShiftReadDto>();
            CreateMap<ShiftCreateDto, Shift>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.UserShifts, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedByUser, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.Mode = src.Mode == "Strict" ? ShiftMode.Strict : ShiftMode.NonStrict;
                });
            CreateMap<User, UserReadDto>();
            CreateMap<UserShift, UserShiftReadDto>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Shift, opt => opt.MapFrom(src => src.Shift));

            // UserShift mappings
            CreateMap<UserShift, UserShiftReadDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : null))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : null))
                .ForMember(dest => dest.ShiftName, opt => opt.MapFrom(src => src.Shift != null ? src.Shift.Name : null))
                .ForMember(dest => dest.ShiftStartTime, opt => opt.MapFrom(src => src.Shift != null ? 
                    DateTime.Today.Add(src.Shift.StartTime) : DateTime.MinValue))
                .ForMember(dest => dest.ShiftEndTime, opt => opt.MapFrom(src => src.Shift != null ? 
                    DateTime.Today.Add(src.Shift.EndTime) : DateTime.MinValue));

            CreateMap<UserShiftAssignDto, UserShift>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Shift, opt => opt.Ignore());
        }
    }
}