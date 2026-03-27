using AutoMapper;
using TechnicianApi.Core.DTOs.Fleet;
using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class MaterialVariantService : IMaterialVariantService
{
    private readonly IRepository<MaterialVariant> _repository;
    private readonly IMapper _mapper;

    public MaterialVariantService(IRepository<MaterialVariant> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<MaterialVariantResponseDto?> GetByIdAsync(string id)
    {
        var variant = await _repository.GetByIdAsync(id);
        return variant == null ? null : _mapper.Map<MaterialVariantResponseDto>(variant);
    }

    public async Task<PagedResponseDto<MaterialVariantResponseDto>> GetPagedAsync(
        int pageNumber, int pageSize, string? materialId = null)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            pageNumber,
            pageSize,
            filter: v => materialId == null || v.MaterialId == materialId
        );

        var dtos = _mapper.Map<IEnumerable<MaterialVariantResponseDto>>(items);

        return new PagedResponseDto<MaterialVariantResponseDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<MaterialVariantResponseDto>> GetByMaterialIdAsync(string materialId)
    {
        var variants = await _repository.FindAsync(v => v.MaterialId == materialId);
        return _mapper.Map<IEnumerable<MaterialVariantResponseDto>>(variants);
    }

    public async Task<MaterialVariantResponseDto> CreateAsync(CreateMaterialVariantDto dto)
    {
        var variant = _mapper.Map<MaterialVariant>(dto);
        var created = await _repository.CreateAsync(variant);
        return _mapper.Map<MaterialVariantResponseDto>(created);
    }

    public async Task<MaterialVariantResponseDto?> UpdateAsync(string id, CreateMaterialVariantDto dto)
    {
        var variant = await _repository.GetByIdAsync(id);
        if (variant == null) return null;

        _mapper.Map(dto, variant);
        var updated = await _repository.UpdateAsync(variant);
        return _mapper.Map<MaterialVariantResponseDto>(updated);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }
}
