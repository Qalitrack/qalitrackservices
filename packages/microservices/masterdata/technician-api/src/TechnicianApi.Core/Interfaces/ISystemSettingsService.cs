using TechnicianApi.Core.DTOs.SystemSettings;

namespace TechnicianApi.Core.Interfaces;

public interface ISystemSettingsService
{
    Task<SystemSettingsResponseDto?> GetSettingsAsync();
    Task<SystemSettingsResponseDto> UpdateSettingsAsync(UpdateSystemSettingsDto dto);
}
