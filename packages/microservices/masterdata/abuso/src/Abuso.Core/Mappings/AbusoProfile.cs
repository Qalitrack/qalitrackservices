using AutoMapper;
using Abuso.Core.DTOs;
using Abuso.Core.Entities;

namespace Abuso.Core.Mappings;

public class AbusoProfile : Profile
{
    public AbusoProfile()
    {
        CreateMap<Abuso.Core.Entities.Abuso, AbusoReadDto>();
        CreateMap<CreateAbusoDto, Abuso.Core.Entities.Abuso>();
        CreateMap<UpdateAbusoDto, Abuso.Core.Entities.Abuso>();
    }
}