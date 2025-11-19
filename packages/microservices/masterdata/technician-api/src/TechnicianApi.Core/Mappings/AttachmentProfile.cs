using AutoMapper;
using TechnicianApi.Core.DTOs.Attachment;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Mappings;

public class AttachmentProfile : Profile
{
    public AttachmentProfile()
    {
        CreateMap<Attachment, AttachmentDto>()
            .ForMember(dest => dest.FileUrl, opt => opt.Ignore());
    }
}
