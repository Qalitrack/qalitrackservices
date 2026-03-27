using AutoMapper;
using TechnicianApi.Core.DTOs.Trip;
using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class TripService : ITripService
{
    private readonly IRepository<Trip> _repository;
    private readonly IRepository<Expense> _expenseRepository;
    private readonly IRepository<TripMaterial> _tripMaterialRepository;
    private readonly IMapper _mapper;

    public TripService(
        IRepository<Trip> repository,
        IRepository<Expense> expenseRepository,
        IRepository<TripMaterial> tripMaterialRepository,
        IMapper mapper)
    {
        _repository = repository;
        _expenseRepository = expenseRepository;
        _tripMaterialRepository = tripMaterialRepository;
        _mapper = mapper;
    }

    public async Task<TripResponseDto?> GetByIdAsync(string id)
    {
        var trip = await _repository.GetByIdAsync(id);
        return trip == null ? null : _mapper.Map<TripResponseDto>(trip);
    }

    public async Task<PagedResponseDto<TripResponseDto>> GetPagedAsync(
        int pageNumber, int pageSize, string? driverId = null, string? truckId = null, string? status = null)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            pageNumber,
            pageSize,
            filter: t =>
                (driverId == null || t.DriverId == driverId) &&
                (truckId == null || t.TruckId == truckId) &&
                (status == null || t.Status.ToString() == status)
        );

        var dtos = _mapper.Map<IEnumerable<TripResponseDto>>(items);

        return new PagedResponseDto<TripResponseDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<TripResponseDto> CreateAsync(CreateTripDto dto)
    {
        var trip = _mapper.Map<Trip>(dto);
        var created = await _repository.CreateAsync(trip);
        return _mapper.Map<TripResponseDto>(created);
    }

    public async Task<TripResponseDto?> UpdateAsync(string id, UpdateTripDto dto)
    {
        var trip = await _repository.GetByIdAsync(id);
        if (trip == null) return null;

        _mapper.Map(dto, trip);
        var updated = await _repository.UpdateAsync(trip);
        return _mapper.Map<TripResponseDto>(updated);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<TripResponseDto?> StartTripAsync(string id, StartTripDto dto)
    {
        var trip = await _repository.GetByIdAsync(id);
        if (trip == null) return null;

        trip.StartMileage = dto.StartMileage;
        trip.ProofImageUrl = dto.ProofImageUrl;
        trip.Status = TripStatus.InProgress;

        var updated = await _repository.UpdateAsync(trip);
        return _mapper.Map<TripResponseDto>(updated);
    }

    public async Task<TripResponseDto?> EndTripAsync(string id, EndTripDto dto)
    {
        var trip = await _repository.GetByIdAsync(id);
        if (trip == null) return null;

        trip.EndMileage = dto.EndMileage;
        trip.ProofEndImageUrl = dto.ProofEndImageUrl;
        trip.Status = TripStatus.Completed;

        if (trip.StartMileage.HasValue && trip.EndMileage.HasValue)
        {
            trip.TotalMileage = trip.EndMileage.Value - trip.StartMileage.Value;
        }

        await RecalculateTotalCostInternalAsync(trip);
        var updated = await _repository.UpdateAsync(trip);
        return _mapper.Map<TripResponseDto>(updated);
    }

    public async Task<TripResponseDto?> RecalculateTotalCostAsync(string id)
    {
        var trip = await _repository.GetByIdAsync(id);
        if (trip == null) return null;

        await RecalculateTotalCostInternalAsync(trip);
        var updated = await _repository.UpdateAsync(trip);
        return _mapper.Map<TripResponseDto>(updated);
    }

    public async Task<TripResponseDto?> UpdateProofImageAsync(string id, string imageUrl)
    {
        var trip = await _repository.GetByIdAsync(id);
        if (trip == null) return null;

        trip.ProofImageUrl = imageUrl;
        var updated = await _repository.UpdateAsync(trip);
        return _mapper.Map<TripResponseDto>(updated);
    }

    public async Task<TripResponseDto?> UpdateEndProofImageAsync(string id, string imageUrl)
    {
        var trip = await _repository.GetByIdAsync(id);
        if (trip == null) return null;

        trip.ProofEndImageUrl = imageUrl;
        var updated = await _repository.UpdateAsync(trip);
        return _mapper.Map<TripResponseDto>(updated);
    }

    public async Task<TripResponseDto?> AddMaterialLoadingPhotoAsync(string id, string imageUrl)
    {
        var trip = await _repository.GetByIdAsync(id);
        if (trip == null) return null;

        // Parse existing photos or create new array
        var photos = new List<string>();
        if (!string.IsNullOrEmpty(trip.MaterialLoadingPhotosJson))
        {
            photos = System.Text.Json.JsonSerializer.Deserialize<List<string>>(trip.MaterialLoadingPhotosJson) ?? new List<string>();
        }

        photos.Add(imageUrl);
        trip.MaterialLoadingPhotosJson = System.Text.Json.JsonSerializer.Serialize(photos);

        var updated = await _repository.UpdateAsync(trip);
        return _mapper.Map<TripResponseDto>(updated);
    }

    private async Task RecalculateTotalCostInternalAsync(Trip trip)
    {
        var expenses = await _expenseRepository.FindAsync(e => e.TripId == trip.Id);
        var tripMaterials = await _tripMaterialRepository.FindAsync(tm => tm.TripId == trip.Id);

        trip.TotalCost =
            expenses.Sum(e => e.Amount) +
            (trip.MaterialCost ?? 0) +
            tripMaterials.Sum(tm => tm.TotalCost);
    }
}
