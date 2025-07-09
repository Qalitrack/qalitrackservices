using AutoMapper;
using TestService.Core.DTOs;
using TestService.Core.Entities;
using TestService.Core.Interfaces;

namespace TestService.Core.Services;

public class ItemService : IItemService
{
    private readonly IItemRepository _itemRepository;
    private readonly IMapper _mapper;

    public ItemService(IItemRepository itemRepository, IMapper mapper)
    {
        _itemRepository = itemRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ItemReadDto>> GetAllAsync()
    {
        var items = await _itemRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ItemReadDto>>(items);
    }

    public async Task<ItemReadDto?> GetByIdAsync(string id)
    {
        var item = await _itemRepository.GetByIdAsync(id);
        return item == null ? null : _mapper.Map<ItemReadDto>(item);
    }

    public async Task<ItemReadDto> CreateAsync(CreateItemDto dto)
    {
        var item = _mapper.Map<Item>(dto);
        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        
        var createdItem = await _itemRepository.CreateAsync(item);
        return _mapper.Map<ItemReadDto>(createdItem);
    }

    public async Task<ItemReadDto?> UpdateAsync(string id, UpdateItemDto dto)
    {
        var existingItem = await _itemRepository.GetByIdAsync(id);
        if (existingItem == null)
        {
            return null;
        }

        _mapper.Map(dto, existingItem);
        existingItem.UpdatedAt = DateTime.UtcNow;
        
        var updatedItem = await _itemRepository.UpdateAsync(existingItem);
        return updatedItem == null ? null : _mapper.Map<ItemReadDto>(updatedItem);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _itemRepository.DeleteAsync(id);
    }

    public async Task<bool> IsNameAvailableAsync(string name)
    {
        return await _itemRepository.IsNameAvailableAsync(name);
    }
}