using AutoMapper;
using TechnicianApi.Core.DTOs.Settings;
using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class LicenseClassService : ILicenseClassService
{
    private readonly IRepository<LicenseClass> _repository;
    private readonly IMapper _mapper;

    public LicenseClassService(IRepository<LicenseClass> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<LicenseClassResponseDto?> GetByIdAsync(string id)
    {
        var licenseClass = await _repository.GetByIdAsync(id);
        return licenseClass == null ? null : _mapper.Map<LicenseClassResponseDto>(licenseClass);
    }

    public async Task<PagedResponseDto<LicenseClassResponseDto>> GetPagedAsync(
        int pageNumber, int pageSize, string? searchTerm = null)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            pageNumber,
            pageSize,
            filter: l => searchTerm == null || l.Name.Contains(searchTerm)
        );

        var dtos = _mapper.Map<IEnumerable<LicenseClassResponseDto>>(items);

        return new PagedResponseDto<LicenseClassResponseDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<LicenseClassResponseDto>> GetAllAsync()
    {
        var licenseClasses = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<LicenseClassResponseDto>>(licenseClasses);
    }

    public async Task<LicenseClassResponseDto> CreateAsync(CreateLicenseClassDto dto)
    {
        var licenseClass = _mapper.Map<LicenseClass>(dto);
        var created = await _repository.CreateAsync(licenseClass);
        return _mapper.Map<LicenseClassResponseDto>(created);
    }

    public async Task<LicenseClassResponseDto?> UpdateAsync(string id, CreateLicenseClassDto dto)
    {
        var licenseClass = await _repository.GetByIdAsync(id);
        if (licenseClass == null) return null;

        _mapper.Map(dto, licenseClass);
        var updated = await _repository.UpdateAsync(licenseClass);
        return _mapper.Map<LicenseClassResponseDto>(updated);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }
}
