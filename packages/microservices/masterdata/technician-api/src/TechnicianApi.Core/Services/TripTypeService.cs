using AutoMapper;
using TechnicianApi.Core.DTOs.Trip;
using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class TripTypeService : ITripTypeService
{
    private readonly IRepository<TripType> _repository;
    private readonly IMapper _mapper;

    public TripTypeService(IRepository<TripType> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<TripTypeResponseDto?> GetByIdAsync(string id)
    {
        var tripType = await _repository.GetByIdAsync(id);
        return tripType == null ? null : _mapper.Map<TripTypeResponseDto>(tripType);
    }

    public async Task<PagedResponseDto<TripTypeResponseDto>> GetPagedAsync(
        int pageNumber, int pageSize, bool? isActive = null)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            pageNumber,
            pageSize,
            filter: t => !isActive.HasValue || t.IsActive == isActive.Value
        );

        var dtos = _mapper.Map<IEnumerable<TripTypeResponseDto>>(items);

        return new PagedResponseDto<TripTypeResponseDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<TripTypeResponseDto>> GetAllActiveAsync()
    {
        var tripTypes = await _repository.FindAsync(t => t.IsActive);
        return _mapper.Map<IEnumerable<TripTypeResponseDto>>(tripTypes);
    }

    public async Task<TripTypeResponseDto> CreateAsync(CreateTripTypeDto dto)
    {
        var tripType = _mapper.Map<TripType>(dto);
        var created = await _repository.CreateAsync(tripType);
        return _mapper.Map<TripTypeResponseDto>(created);
    }

    public async Task<TripTypeResponseDto?> UpdateAsync(string id, CreateTripTypeDto dto)
    {
        var tripType = await _repository.GetByIdAsync(id);
        if (tripType == null) return null;

        _mapper.Map(dto, tripType);
        var updated = await _repository.UpdateAsync(tripType);
        return _mapper.Map<TripTypeResponseDto>(updated);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }
}
