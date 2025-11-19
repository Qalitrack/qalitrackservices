using TechnicianApi.Core.DTOs.CheckIn;

namespace TechnicianApi.Core.Interfaces;

public interface ICheckInService
{
    Task<CheckInResponseDto?> GetByIdAsync(string id);
    Task<CheckInResponseDto?> GetByAssignmentIdAsync(string assignmentId);
    Task<CheckInResponseDto> CreateAsync(CreateCheckInDto dto);
    Task<bool> DeleteAsync(string id);
}
