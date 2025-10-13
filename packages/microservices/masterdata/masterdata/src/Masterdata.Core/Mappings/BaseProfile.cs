using AutoMapper;
using Masterdata.Core.DTOs;
using Masterdata.Core.Entities;

namespace Masterdata.Core.Mappings;

public class BaseProfile : Profile
{
    public BaseProfile()
    {
        CreateMap<Masterdata.Core.Entities.Base, BaseReadDto>();
        CreateMap<CreateBaseDto, Masterdata.Core.Entities.Base>();
        CreateMap<UpdateBaseDto, Masterdata.Core.Entities.Base>();
    }
}