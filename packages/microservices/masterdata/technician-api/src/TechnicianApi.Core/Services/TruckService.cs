using AutoMapper;
using TechnicianApi.Core.DTOs.Fleet;
using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class TruckService : ITruckService
{
    private readonly IRepository<Truck> _repository;
    private readonly IMapper _mapper;

    public TruckService(IRepository<Truck> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<TruckResponseDto?> GetByIdAsync(string id)
    {
        var truck = await _repository.GetByIdAsync(id);
        return truck == null ? null : _mapper.Map<TruckResponseDto>(truck);
    }

    public async Task<PagedResponseDto<TruckResponseDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(pageNumber, pageSize);
        var dtos = _mapper.Map<IEnumerable<TruckResponseDto>>(items);

        return new PagedResponseDto<TruckResponseDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<TruckResponseDto> CreateAsync(CreateTruckDto dto)
    {
        var truck = _mapper.Map<Truck>(dto);
        var created = await _repository.CreateAsync(truck);
        return _mapper.Map<TruckResponseDto>(created);
    }

    public async Task<TruckResponseDto?> UpdateAsync(string id, CreateTruckDto dto)
    {
        var truck = await _repository.GetByIdAsync(id);
        if (truck == null) return null;

        truck.LicensePlate = dto.LicensePlate;
        truck.Model = dto.Model;
        truck.DriverId = dto.DriverId;

        var updated = await _repository.UpdateAsync(truck);
        return _mapper.Map<TruckResponseDto>(updated);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }
}
