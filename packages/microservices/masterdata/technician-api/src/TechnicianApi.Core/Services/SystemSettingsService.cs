using AutoMapper;
using TechnicianApi.Core.DTOs.SystemSettings;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class SystemSettingsService : ISystemSettingsService
{
    private readonly IRepository<SystemSettings> _repository;
    private readonly IMapper _mapper;

    public SystemSettingsService(IRepository<SystemSettings> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<SystemSettingsResponseDto?> GetSettingsAsync()
    {
        var all = await _repository.GetAllAsync();
        var settings = all.FirstOrDefault();

        if (settings == null)
        {
            // Create default settings if none exist
            settings = new SystemSettings();
            settings = await _repository.CreateAsync(settings);
        }

        return _mapper.Map<SystemSettingsResponseDto>(settings);
    }

    public async Task<SystemSettingsResponseDto> UpdateSettingsAsync(UpdateSystemSettingsDto dto)
    {
        var all = await _repository.GetAllAsync();
        var settings = all.FirstOrDefault();

        if (settings == null)
        {
            settings = new SystemSettings();
            settings = await _repository.CreateAsync(settings);
        }

        _mapper.Map(dto, settings);
        var updated = await _repository.UpdateAsync(settings);
        return _mapper.Map<SystemSettingsResponseDto>(updated);
    }
}
