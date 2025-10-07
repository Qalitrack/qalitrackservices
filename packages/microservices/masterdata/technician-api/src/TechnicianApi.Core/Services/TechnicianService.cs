using AutoMapper;
using TechnicianApi.Core.DTOs;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class TechnicianService : ITechnicianService
{
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IMapper _mapper;

    public TechnicianService(ITechnicianRepository technicianRepository, IMapper mapper)
    {
        _technicianRepository = technicianRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TechnicianReadDto>> GetAllAsync()
    {
        var technicians = await _technicianRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<TechnicianReadDto>>(technicians);
    }

    public async Task<TechnicianReadDto?> GetByIdAsync(string id)
    {
        var technician = await _technicianRepository.GetByIdAsync(id);
        return technician == null ? null : _mapper.Map<TechnicianReadDto>(technician);
    }

    public async Task<TechnicianReadDto> CreateAsync(CreateTechnicianDto dto)
    {
        var technician = _mapper.Map<TechnicianApi.Core.Entities.Technician>(dto);
        technician.CreatedAt = DateTime.UtcNow;
        technician.UpdatedAt = DateTime.UtcNow;
        
        var createdTechnician = await _technicianRepository.CreateAsync(technician);
        return _mapper.Map<TechnicianReadDto>(createdTechnician);
    }

    public async Task<TechnicianReadDto?> UpdateAsync(string id, UpdateTechnicianDto dto)
    {
        var existingTechnician = await _technicianRepository.GetByIdAsync(id);
        if (existingTechnician == null)
        {
            return null;
        }

        _mapper.Map(dto, existingTechnician);
        existingTechnician.UpdatedAt = DateTime.UtcNow;
        
        var updatedTechnician = await _technicianRepository.UpdateAsync(existingTechnician);
        return updatedTechnician == null ? null : _mapper.Map<TechnicianReadDto>(updatedTechnician);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _technicianRepository.DeleteAsync(id);
    }

    public async Task<bool> IsNameAvailableAsync(string name)
    {
        return await _technicianRepository.IsNameAvailableAsync(name);
    }
}