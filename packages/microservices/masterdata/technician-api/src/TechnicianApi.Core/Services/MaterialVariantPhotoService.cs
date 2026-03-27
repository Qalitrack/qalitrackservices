using AutoMapper;
using TechnicianApi.Core.DTOs.Fleet;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class MaterialVariantPhotoService : IMaterialVariantPhotoService
{
    private readonly IRepository<MaterialVariantPhoto> _repository;
    private readonly IMapper _mapper;

    public MaterialVariantPhotoService(IRepository<MaterialVariantPhoto> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<MaterialVariantPhotoResponseDto?> GetByIdAsync(string id)
    {
        var photo = await _repository.GetByIdAsync(id);
        return photo == null ? null : _mapper.Map<MaterialVariantPhotoResponseDto>(photo);
    }

    public async Task<IEnumerable<MaterialVariantPhotoResponseDto>> GetByVariantIdAsync(string variantId)
    {
        var photos = await _repository.FindAsync(p => p.MaterialVariantId == variantId);
        return _mapper.Map<IEnumerable<MaterialVariantPhotoResponseDto>>(photos);
    }

    public async Task<MaterialVariantPhotoResponseDto> CreateAsync(CreateMaterialVariantPhotoDto dto)
    {
        var photo = _mapper.Map<MaterialVariantPhoto>(dto);
        var created = await _repository.CreateAsync(photo);
        return _mapper.Map<MaterialVariantPhotoResponseDto>(created);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }
}
