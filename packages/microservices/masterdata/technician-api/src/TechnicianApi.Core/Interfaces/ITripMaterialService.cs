using TechnicianApi.Core.DTOs.Trip;

namespace TechnicianApi.Core.Interfaces;

public interface ITripMaterialService
{
    Task<TripMaterialResponseDto?> GetByIdAsync(string id);
    Task<IEnumerable<TripMaterialResponseDto>> GetByTripIdAsync(string tripId);
    Task<TripMaterialResponseDto> CreateAsync(CreateTripMaterialDto dto);
    Task<TripMaterialResponseDto?> UpdateAsync(string id, CreateTripMaterialDto dto);
    Task<bool> DeleteAsync(string id);
}
