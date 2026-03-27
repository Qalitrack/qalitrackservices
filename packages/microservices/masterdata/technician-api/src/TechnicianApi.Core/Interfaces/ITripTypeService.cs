using TechnicianApi.Core.DTOs.Trip;
using TechnicianApi.Core.DTOs.Common;

namespace TechnicianApi.Core.Interfaces;

public interface ITripTypeService
{
    Task<TripTypeResponseDto?> GetByIdAsync(string id);
    Task<PagedResponseDto<TripTypeResponseDto>> GetPagedAsync(int pageNumber, int pageSize, bool? isActive = null);
    Task<IEnumerable<TripTypeResponseDto>> GetAllActiveAsync();
    Task<TripTypeResponseDto> CreateAsync(CreateTripTypeDto dto);
    Task<TripTypeResponseDto?> UpdateAsync(string id, CreateTripTypeDto dto);
    Task<bool> DeleteAsync(string id);
}
