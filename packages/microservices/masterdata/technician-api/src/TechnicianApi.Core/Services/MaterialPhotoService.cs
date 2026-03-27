using AutoMapper;
using TechnicianApi.Core.DTOs.Fleet;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class MaterialPhotoService : IMaterialPhotoService
{
    private readonly IRepository<MaterialPhoto> _repository;
    private readonly IMapper _mapper;

    public MaterialPhotoService(IRepository<MaterialPhoto> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<MaterialPhotoResponseDto?> GetByIdAsync(string id)
    {
        var photo = await _repository.GetByIdAsync(id);
        return photo == null ? null : _mapper.Map<MaterialPhotoResponseDto>(photo);
    }

    public async Task<IEnumerable<MaterialPhotoResponseDto>> GetByMaterialIdAsync(string materialId)
    {
        var photos = await _repository.FindAsync(p => p.MaterialId == materialId);
        return _mapper.Map<IEnumerable<MaterialPhotoResponseDto>>(photos);
    }

    public async Task<MaterialPhotoResponseDto> CreateAsync(CreateMaterialPhotoDto dto)
    {
        var photo = _mapper.Map<MaterialPhoto>(dto);
        var created = await _repository.CreateAsync(photo);
        return _mapper.Map<MaterialPhotoResponseDto>(created);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }
}
