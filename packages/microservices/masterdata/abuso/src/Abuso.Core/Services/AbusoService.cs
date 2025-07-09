using AutoMapper;
using Abuso.Core.DTOs;
using Abuso.Core.Entities;
using Abuso.Core.Interfaces;

namespace Abuso.Core.Services;

public class AbusoService : IAbusoService
{
    private readonly IAbusoRepository _abusoRepository;
    private readonly IMapper _mapper;

    public AbusoService(IAbusoRepository abusoRepository, IMapper mapper)
    {
        _abusoRepository = abusoRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AbusoReadDto>> GetAllAsync()
    {
        var abusos = await _abusoRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<AbusoReadDto>>(abusos);
    }

    public async Task<AbusoReadDto?> GetByIdAsync(string id)
    {
        var abuso = await _abusoRepository.GetByIdAsync(id);
        return abuso == null ? null : _mapper.Map<AbusoReadDto>(abuso);
    }

    public async Task<AbusoReadDto> CreateAsync(CreateAbusoDto dto)
    {
        var abuso = _mapper.Map<Abuso.Core.Entities.Abuso>(dto);
        abuso.CreatedAt = DateTime.UtcNow;
        abuso.UpdatedAt = DateTime.UtcNow;
        
        var createdAbuso = await _abusoRepository.CreateAsync(abuso);
        return _mapper.Map<AbusoReadDto>(createdAbuso);
    }

    public async Task<AbusoReadDto?> UpdateAsync(string id, UpdateAbusoDto dto)
    {
        var existingAbuso = await _abusoRepository.GetByIdAsync(id);
        if (existingAbuso == null)
        {
            return null;
        }

        _mapper.Map(dto, existingAbuso);
        existingAbuso.UpdatedAt = DateTime.UtcNow;
        
        var updatedAbuso = await _abusoRepository.UpdateAsync(existingAbuso);
        return updatedAbuso == null ? null : _mapper.Map<AbusoReadDto>(updatedAbuso);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _abusoRepository.DeleteAsync(id);
    }

    public async Task<bool> IsNameAvailableAsync(string name)
    {
        return await _abusoRepository.IsNameAvailableAsync(name);
    }
}