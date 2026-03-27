using AutoMapper;
using TechnicianApi.Core.DTOs.Driver;
using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class DriverService : IDriverService
{
    private readonly IRepository<Driver> _repository;
    private readonly IMapper _mapper;

    public DriverService(IRepository<Driver> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<DriverResponseDto?> GetByIdAsync(string id)
    {
        var driver = await _repository.GetByIdAsync(id);
        return driver == null ? null : _mapper.Map<DriverResponseDto>(driver);
    }

    public async Task<DriverResponseDto?> GetByUserIdAsync(string userId)
    {
        var driver = await _repository.FirstOrDefaultAsync(d => d.UserId == userId);
        return driver == null ? null : _mapper.Map<DriverResponseDto>(driver);
    }

    public async Task<PagedResponseDto<DriverResponseDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(pageNumber, pageSize);
        var dtos = _mapper.Map<IEnumerable<DriverResponseDto>>(items);

        return new PagedResponseDto<DriverResponseDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<DriverResponseDto> CreateAsync(CreateDriverDto dto)
    {
        var driver = _mapper.Map<Driver>(dto);
        var created = await _repository.CreateAsync(driver);
        return _mapper.Map<DriverResponseDto>(created);
    }

    public async Task<DriverResponseDto?> UpdateAsync(string id, UpdateDriverDto dto)
    {
        var driver = await _repository.GetByIdAsync(id);
        if (driver == null) return null;

        _mapper.Map(dto, driver);
        var updated = await _repository.UpdateAsync(driver);
        return _mapper.Map<DriverResponseDto>(updated);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }
}
