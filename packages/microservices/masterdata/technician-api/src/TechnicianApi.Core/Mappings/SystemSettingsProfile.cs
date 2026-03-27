using AutoMapper;
using TechnicianApi.Core.DTOs.SystemSettings;
using TechnicianApi.Core.Entities;
using System.Text.Json;

namespace TechnicianApi.Core.Mappings;

public class SystemSettingsProfile : Profile
{
    public SystemSettingsProfile()
    {
        CreateMap<SystemSettings, SystemSettingsResponseDto>();

        CreateMap<UpdateSystemSettingsDto, SystemSettings>()
            .ForMember(dest => dest.AutoApproveUserTypesJson,
                opt => opt.MapFrom(src => SerializeUserTypes(src.AutoApproveUserTypes)))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }

    private static string SerializeUserTypes(List<string> userTypes)
    {
        return JsonSerializer.Serialize(userTypes);
    }
}
