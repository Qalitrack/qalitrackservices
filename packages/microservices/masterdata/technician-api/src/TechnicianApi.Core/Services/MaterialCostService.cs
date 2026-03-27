using AutoMapper;
using TechnicianApi.Core.DTOs.Fleet;
using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class MaterialCostService : IMaterialCostService
{
    private readonly IRepository<MaterialCost> _repository;
    private readonly IMapper _mapper;

    public MaterialCostService(IRepository<MaterialCost> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<MaterialCostResponseDto?> GetByIdAsync(string id)
    {
        var cost = await _repository.GetByIdAsync(id);
        return cost == null ? null : _mapper.Map<MaterialCostResponseDto>(cost);
    }

    public async Task<PagedResponseDto<MaterialCostResponseDto>> GetPagedAsync(
        int pageNumber, int pageSize, string? materialId = null)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            pageNumber,
            pageSize,
            filter: c => materialId == null || c.MaterialId == materialId
        );

        var dtos = _mapper.Map<IEnumerable<MaterialCostResponseDto>>(items);

        return new PagedResponseDto<MaterialCostResponseDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<MaterialCostResponseDto>> GetByMaterialIdAsync(string materialId)
    {
        var costs = await _repository.FindAsync(c => c.MaterialId == materialId);
        return _mapper.Map<IEnumerable<MaterialCostResponseDto>>(costs);
    }

    public async Task<MaterialCostResponseDto> CreateAsync(CreateMaterialCostDto dto)
    {
        var cost = _mapper.Map<MaterialCost>(dto);
        var created = await _repository.CreateAsync(cost);
        return _mapper.Map<MaterialCostResponseDto>(created);
    }

    public async Task<MaterialCostResponseDto?> UpdateAsync(string id, CreateMaterialCostDto dto)
    {
        var cost = await _repository.GetByIdAsync(id);
        if (cost == null) return null;

        _mapper.Map(dto, cost);
        var updated = await _repository.UpdateAsync(cost);
        return _mapper.Map<MaterialCostResponseDto>(updated);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }
}
