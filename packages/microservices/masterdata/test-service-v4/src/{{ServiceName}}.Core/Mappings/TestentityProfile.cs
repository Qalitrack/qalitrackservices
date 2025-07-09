using AutoMapper;
using TestServiceV4.Core.DTOs;
using TestServiceV4.Core.Entities;

namespace TestServiceV4.Core.Mappings;

public class TestentityProfile : Profile
{
    public TestentityProfile()
    {
        CreateMap<Testentity, TestentityReadDto>();
        CreateMap<CreateTestentityDto, Testentity>();
        CreateMap<UpdateTestentityDto, Testentity>();
    }
}