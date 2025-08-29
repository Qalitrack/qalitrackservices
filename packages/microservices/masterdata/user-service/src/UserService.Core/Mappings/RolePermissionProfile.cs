using AutoMapper;
using UserService.Core.DTOs.RolePermission;
using UserService.Core.Entities;

namespace UserService.Core.Mappings;

public class RolePermissionProfile : Profile
{
    public RolePermissionProfile()
    {
        CreateMap<RolePermission, RolePermissionDto>()
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
            .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.UpdatedBy))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));
    }
}