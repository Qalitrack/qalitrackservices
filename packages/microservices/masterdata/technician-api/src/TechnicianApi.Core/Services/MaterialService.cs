using AutoMapper;
using TechnicianApi.Core.DTOs.Fleet;
using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class MaterialService : IMaterialService
{
    private readonly IRepository<Material> _repository;
    private readonly IRepository<MaterialVariant> _variantRepository;
    private readonly IMapper _mapper;

    public MaterialService(
        IRepository<Material> repository,
        IRepository<MaterialVariant> variantRepository,
        IMapper mapper)
    {
        _repository = repository;
        _variantRepository = variantRepository;
        _mapper = mapper;
    }

    public async Task<MaterialResponseDto?> GetByIdAsync(string id)
    {
        var material = await _repository.GetByIdAsync(id);
        return material == null ? null : _mapper.Map<MaterialResponseDto>(material);
    }

    public async Task<PagedResponseDto<MaterialResponseDto>> GetPagedAsync(
        int pageNumber, int pageSize, string? searchTerm = null)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            pageNumber,
            pageSize,
            filter: m => searchTerm == null || m.Name.Contains(searchTerm)
        );

        var dtos = _mapper.Map<IEnumerable<MaterialResponseDto>>(items);

        return new PagedResponseDto<MaterialResponseDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<MaterialResponseDto> CreateAsync(CreateMaterialDto dto)
    {
        var material = _mapper.Map<Material>(dto);
        var created = await _repository.CreateAsync(material);
        return _mapper.Map<MaterialResponseDto>(created);
    }

    public async Task<MaterialResponseDto?> UpdateAsync(string id, CreateMaterialDto dto)
    {
        var material = await _repository.GetByIdAsync(id);
        if (material == null) return null;

        material.Name = dto.Name;
        material.Description = dto.Description;

        var updated = await _repository.UpdateAsync(material);
        return _mapper.Map<MaterialResponseDto>(updated);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<MaterialVariantResponseDto>> GetVariantsByMaterialIdAsync(string materialId)
    {
        var variants = await _variantRepository.FindAsync(v => v.MaterialId == materialId);
        return _mapper.Map<IEnumerable<MaterialVariantResponseDto>>(variants);
    }
}
