using AutoMapper;
using TechnicianApi.Core.DTOs.Photo;
using TechnicianApi.Core.Entities;

namespace TechnicianApi.Core.Mappings;

public class PhotoProfile : Profile
{
    public PhotoProfile()
    {
        // Entity to ResponseDto
        CreateMap<Photo, PhotoResponseDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

        // CreateDto to Entity
        CreateMap<CreatePhotoDto, Photo>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<PhotoType>(src.Type)))
            .ForMember(dest => dest.CapturedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Assignment, opt => opt.Ignore());
    }
}
