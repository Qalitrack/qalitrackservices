using AutoMapper;
using TechnicianApi.Core.DTOs.Photo;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class PhotoService : IPhotoService
{
    private readonly IRepository<Photo> _repository;
    private readonly IMapper _mapper;

    public PhotoService(IRepository<Photo> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PhotoResponseDto?> GetByIdAsync(string id)
    {
        var photo = await _repository.GetByIdAsync(id);
        return photo == null ? null : _mapper.Map<PhotoResponseDto>(photo);
    }

    public async Task<IEnumerable<PhotoResponseDto>> GetByAssignmentIdAsync(string assignmentId)
    {
        var photos = await _repository.FindAsync(p => p.AssignmentId == assignmentId);
        return _mapper.Map<IEnumerable<PhotoResponseDto>>(photos);
    }

    public async Task<IEnumerable<PhotoResponseDto>> GetByAssignmentAndTypeAsync(string assignmentId, string type)
    {
        if (!Enum.TryParse<PhotoType>(type, out var photoType))
            return Enumerable.Empty<PhotoResponseDto>();

        var photos = await _repository.FindAsync(p =>
            p.AssignmentId == assignmentId && p.Type == photoType);
        return _mapper.Map<IEnumerable<PhotoResponseDto>>(photos);
    }

    public async Task<PhotoResponseDto> CreateAsync(CreatePhotoDto dto)
    {
        var photo = _mapper.Map<Photo>(dto);
        var created = await _repository.CreateAsync(photo);
        return _mapper.Map<PhotoResponseDto>(created);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }
}
