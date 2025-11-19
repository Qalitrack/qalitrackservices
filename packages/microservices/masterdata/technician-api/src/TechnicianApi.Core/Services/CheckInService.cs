using AutoMapper;
using TechnicianApi.Core.DTOs.CheckIn;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class CheckInService : ICheckInService
{
    private readonly IRepository<CheckIn> _repository;
    private readonly IMapper _mapper;

    public CheckInService(IRepository<CheckIn> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<CheckInResponseDto?> GetByIdAsync(string id)
    {
        var checkIn = await _repository.GetByIdAsync(id);
        return checkIn == null ? null : _mapper.Map<CheckInResponseDto>(checkIn);
    }

    public async Task<CheckInResponseDto?> GetByAssignmentIdAsync(string assignmentId)
    {
        var checkIn = await _repository.FirstOrDefaultAsync(c => c.AssignmentId == assignmentId);
        return checkIn == null ? null : _mapper.Map<CheckInResponseDto>(checkIn);
    }

    public async Task<CheckInResponseDto> CreateAsync(CreateCheckInDto dto)
    {
        var checkIn = _mapper.Map<CheckIn>(dto);
        var created = await _repository.CreateAsync(checkIn);
        return _mapper.Map<CheckInResponseDto>(created);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }
}
