using AutoMapper;
using TechnicianApi.Core.DTOs.Trip;
using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class VehicleMileageService : IVehicleMileageService
{
    private readonly IRepository<VehicleMileage> _repository;
    private readonly IMapper _mapper;

    public VehicleMileageService(IRepository<VehicleMileage> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<VehicleMileageResponseDto?> GetByIdAsync(string id)
    {
        var mileage = await _repository.GetByIdAsync(id);
        return mileage == null ? null : _mapper.Map<VehicleMileageResponseDto>(mileage);
    }

    public async Task<PagedResponseDto<VehicleMileageResponseDto>> GetPagedAsync(
        int pageNumber, int pageSize, string? truckId = null, string? driverId = null)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            pageNumber,
            pageSize,
            filter: m =>
                (truckId == null || m.TruckId == truckId) &&
                (driverId == null || m.DriverId == driverId)
        );

        var dtos = _mapper.Map<IEnumerable<VehicleMileageResponseDto>>(items);

        return new PagedResponseDto<VehicleMileageResponseDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<VehicleMileageResponseDto> CreateAsync(CreateVehicleMileageDto dto)
    {
        var mileage = _mapper.Map<VehicleMileage>(dto);

        // Calculate mileage
        mileage.Mileage = dto.EndMileage - dto.StartMileage;
        mileage.Date = dto.Date ?? DateTime.UtcNow;

        var created = await _repository.CreateAsync(mileage);
        return _mapper.Map<VehicleMileageResponseDto>(created);
    }

    public async Task<VehicleMileageResponseDto?> UpdateAsync(string id, CreateVehicleMileageDto dto)
    {
        var mileage = await _repository.GetByIdAsync(id);
        if (mileage == null) return null;

        _mapper.Map(dto, mileage);

        // Recalculate mileage
        mileage.Mileage = dto.EndMileage - dto.StartMileage;
        if (dto.Date.HasValue)
            mileage.Date = dto.Date.Value;

        var updated = await _repository.UpdateAsync(mileage);
        return _mapper.Map<VehicleMileageResponseDto>(updated);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<VehicleMileageResponseDto?> UpdateProofImageAsync(string id, string imageUrl)
    {
        var mileage = await _repository.GetByIdAsync(id);
        if (mileage == null) return null;

        mileage.ProofImageUrl = imageUrl;
        var updated = await _repository.UpdateAsync(mileage);
        return _mapper.Map<VehicleMileageResponseDto>(updated);
    }

    public async Task<VehicleMileageResponseDto?> UpdateProofEndImageAsync(string id, string imageUrl)
    {
        var mileage = await _repository.GetByIdAsync(id);
        if (mileage == null) return null;

        mileage.ProofEndImageUrl = imageUrl;
        var updated = await _repository.UpdateAsync(mileage);
        return _mapper.Map<VehicleMileageResponseDto>(updated);
    }
}
