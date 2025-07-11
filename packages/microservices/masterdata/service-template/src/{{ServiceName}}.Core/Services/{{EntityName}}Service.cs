using AutoMapper;
using {{ServiceName}}.Core.DTOs;
using {{ServiceName}}.Core.Entities;
using {{ServiceName}}.Core.Interfaces;

namespace {{ServiceName}}.Core.Services;

public class {{EntityName}}Service : I{{EntityName}}Service
{
    private readonly I{{EntityName}}Repository _{{entityName}}Repository;
    private readonly IMapper _mapper;

    public {{EntityName}}Service(I{{EntityName}}Repository {{entityName}}Repository, IMapper mapper)
    {
        _{{entityName}}Repository = {{entityName}}Repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<{{EntityName}}ReadDto>> GetAllAsync()
    {
        var {{entityName}}s = await _{{entityName}}Repository.GetAllAsync();
        return _mapper.Map<IEnumerable<{{EntityName}}ReadDto>>({{entityName}}s);
    }

    public async Task<{{EntityName}}ReadDto?> GetByIdAsync(string id)
    {
        var {{entityName}} = await _{{entityName}}Repository.GetByIdAsync(id);
        return {{entityName}} == null ? null : _mapper.Map<{{EntityName}}ReadDto>({{entityName}});
    }

    public async Task<{{EntityName}}ReadDto> CreateAsync(Create{{EntityName}}Dto dto)
    {
        var {{entityName}} = _mapper.Map<{{ServiceName}}.Core.Entities.{{EntityName}}>(dto);
        {{entityName}}.CreatedAt = DateTime.UtcNow;
        {{entityName}}.UpdatedAt = DateTime.UtcNow;
        
        var created{{EntityName}} = await _{{entityName}}Repository.CreateAsync({{entityName}});
        return _mapper.Map<{{EntityName}}ReadDto>(created{{EntityName}});
    }

    public async Task<{{EntityName}}ReadDto?> UpdateAsync(string id, Update{{EntityName}}Dto dto)
    {
        var existing{{EntityName}} = await _{{entityName}}Repository.GetByIdAsync(id);
        if (existing{{EntityName}} == null)
        {
            return null;
        }

        _mapper.Map(dto, existing{{EntityName}});
        existing{{EntityName}}.UpdatedAt = DateTime.UtcNow;
        
        var updated{{EntityName}} = await _{{entityName}}Repository.UpdateAsync(existing{{EntityName}});
        return updated{{EntityName}} == null ? null : _mapper.Map<{{EntityName}}ReadDto>(updated{{EntityName}});
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _{{entityName}}Repository.DeleteAsync(id);
    }

    public async Task<bool> IsNameAvailableAsync(string name)
    {
        return await _{{entityName}}Repository.IsNameAvailableAsync(name);
    }
}