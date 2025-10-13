using AutoMapper;
using Masterdata.Core.DTOs;
using Masterdata.Core.Entities;
using Masterdata.Core.Interfaces;

namespace Masterdata.Core.Services;

public class BaseService : IBaseService
{
    private readonly IBaseRepository _baseRepository;
    private readonly IMapper _mapper;

    public BaseService(IBaseRepository baseRepository, IMapper mapper)
    {
        _baseRepository = baseRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<BaseReadDto>> GetAllAsync()
    {
        var bases = await _baseRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<BaseReadDto>>(bases);
    }

    public async Task<BaseReadDto?> GetByIdAsync(string id)
    {
        var base = await _baseRepository.GetByIdAsync(id);
        return base == null ? null : _mapper.Map<BaseReadDto>(base);
    }

    public async Task<BaseReadDto> CreateAsync(CreateBaseDto dto)
    {
        var base = _mapper.Map<Masterdata.Core.Entities.Base>(dto);
        base.CreatedAt = DateTime.UtcNow;
        base.UpdatedAt = DateTime.UtcNow;
        
        var createdBase = await _baseRepository.CreateAsync(base);
        return _mapper.Map<BaseReadDto>(createdBase);
    }

    public async Task<BaseReadDto?> UpdateAsync(string id, UpdateBaseDto dto)
    {
        var existingBase = await _baseRepository.GetByIdAsync(id);
        if (existingBase == null)
        {
            return null;
        }

        _mapper.Map(dto, existingBase);
        existingBase.UpdatedAt = DateTime.UtcNow;
        
        var updatedBase = await _baseRepository.UpdateAsync(existingBase);
        return updatedBase == null ? null : _mapper.Map<BaseReadDto>(updatedBase);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _baseRepository.DeleteAsync(id);
    }

    public async Task<bool> IsNameAvailableAsync(string name)
    {
        return await _baseRepository.IsNameAvailableAsync(name);
    }
}