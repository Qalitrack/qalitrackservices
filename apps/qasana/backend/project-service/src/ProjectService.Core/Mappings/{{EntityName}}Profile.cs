using AutoMapper;
using {{ServiceName}}.Core.DTOs;
using {{ServiceName}}.Core.Entities;

namespace {{ServiceName}}.Core.Mappings;

public class {{EntityName}}Profile : Profile
{
    public {{EntityName}}Profile()
    {
        CreateMap<{{ServiceName}}.Core.Entities.{{EntityName}}, {{EntityName}}ReadDto>();
        CreateMap<Create{{EntityName}}Dto, {{ServiceName}}.Core.Entities.{{EntityName}}>();
        CreateMap<Update{{EntityName}}Dto, {{ServiceName}}.Core.Entities.{{EntityName}}>();
    }
}