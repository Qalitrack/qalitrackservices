using AutoMapper;
using TechnicianApi.Core.DTOs;
using TechnicianApi.Core.Entities;

namespace TechnicianApi.Core.Mappings;

public class TechnicianProfile : Profile
{
    public TechnicianProfile()
    {
        CreateMap<TechnicianApi.Core.Entities.Technician, TechnicianReadDto>();
        CreateMap<CreateTechnicianDto, TechnicianApi.Core.Entities.Technician>();
        CreateMap<UpdateTechnicianDto, TechnicianApi.Core.Entities.Technician>();
    }
}