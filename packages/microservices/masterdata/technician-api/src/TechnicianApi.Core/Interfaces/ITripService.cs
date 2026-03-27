using TechnicianApi.Core.DTOs.Trip;
using TechnicianApi.Core.DTOs.Common;

namespace TechnicianApi.Core.Interfaces;

public interface ITripService
{
    Task<TripResponseDto?> GetByIdAsync(string id);
    Task<PagedResponseDto<TripResponseDto>> GetPagedAsync(int pageNumber, int pageSize, string? driverId = null, string? truckId = null, string? status = null);
    Task<TripResponseDto> CreateAsync(CreateTripDto dto);
    Task<TripResponseDto?> UpdateAsync(string id, UpdateTripDto dto);
    Task<bool> DeleteAsync(string id);
    Task<TripResponseDto?> StartTripAsync(string id, StartTripDto dto);
    Task<TripResponseDto?> EndTripAsync(string id, EndTripDto dto);
    Task<TripResponseDto?> RecalculateTotalCostAsync(string id);
    Task<TripResponseDto?> UpdateProofImageAsync(string id, string imageUrl);
    Task<TripResponseDto?> UpdateEndProofImageAsync(string id, string imageUrl);
    Task<TripResponseDto?> AddMaterialLoadingPhotoAsync(string id, string imageUrl);
}
