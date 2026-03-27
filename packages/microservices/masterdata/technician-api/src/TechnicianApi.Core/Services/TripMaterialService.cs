using AutoMapper;
using TechnicianApi.Core.DTOs.Trip;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class TripMaterialService : ITripMaterialService
{
    private readonly IRepository<TripMaterial> _repository;
    private readonly ITripService _tripService;
    private readonly IMapper _mapper;

    public TripMaterialService(
        IRepository<TripMaterial> repository,
        ITripService tripService,
        IMapper mapper)
    {
        _repository = repository;
        _tripService = tripService;
        _mapper = mapper;
    }

    public async Task<TripMaterialResponseDto?> GetByIdAsync(string id)
    {
        var tripMaterial = await _repository.GetByIdAsync(id);
        return tripMaterial == null ? null : _mapper.Map<TripMaterialResponseDto>(tripMaterial);
    }

    public async Task<IEnumerable<TripMaterialResponseDto>> GetByTripIdAsync(string tripId)
    {
        var tripMaterials = await _repository.FindAsync(tm => tm.TripId == tripId);
        return _mapper.Map<IEnumerable<TripMaterialResponseDto>>(tripMaterials);
    }

    public async Task<TripMaterialResponseDto> CreateAsync(CreateTripMaterialDto dto)
    {
        var tripMaterial = _mapper.Map<TripMaterial>(dto);
        tripMaterial.TotalCost = (dto.Quantity ?? 1) * dto.UnitCost;

        var created = await _repository.CreateAsync(tripMaterial);

        // Recalculate trip total cost
        await _tripService.RecalculateTotalCostAsync(dto.TripId);

        return _mapper.Map<TripMaterialResponseDto>(created);
    }

    public async Task<TripMaterialResponseDto?> UpdateAsync(string id, CreateTripMaterialDto dto)
    {
        var tripMaterial = await _repository.GetByIdAsync(id);
        if (tripMaterial == null) return null;

        tripMaterial.MaterialId = dto.MaterialId;
        tripMaterial.MaterialVariantId = dto.MaterialVariantId;
        tripMaterial.Quantity = dto.Quantity;
        tripMaterial.UnitCost = dto.UnitCost;
        tripMaterial.TotalCost = (dto.Quantity ?? 1) * dto.UnitCost;
        tripMaterial.Notes = dto.Notes;

        var updated = await _repository.UpdateAsync(tripMaterial);

        // Recalculate trip total cost
        await _tripService.RecalculateTotalCostAsync(tripMaterial.TripId);

        return _mapper.Map<TripMaterialResponseDto>(updated);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var tripMaterial = await _repository.GetByIdAsync(id);
        var tripId = tripMaterial?.TripId;

        var result = await _repository.DeleteAsync(id);

        // Recalculate trip cost after deleting
        if (result && tripId != null)
        {
            await _tripService.RecalculateTotalCostAsync(tripId);
        }

        return result;
    }
}
