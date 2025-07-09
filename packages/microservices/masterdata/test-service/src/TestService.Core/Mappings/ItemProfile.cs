using AutoMapper;
using TestService.Core.DTOs;
using TestService.Core.Entities;

namespace TestService.Core.Mappings;

public class ItemProfile : Profile
{
    public ItemProfile()
    {
        CreateMap<Item, ItemReadDto>();
        CreateMap<CreateItemDto, Item>();
        CreateMap<UpdateItemDto, Item>();
    }
}