using AutoMapper;
using TestServiceV4.Core.DTOs;
using TestServiceV4.Core.Entities;
using TestServiceV4.Core.Interfaces;

namespace TestServiceV4.Core.Services;

public class TestentityService : ITestentityService
{
    private readonly ITestentityRepository _testentityRepository;
    private readonly IMapper _mapper;

    public TestentityService(ITestentityRepository testentityRepository, IMapper mapper)
    {
        _testentityRepository = testentityRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TestentityReadDto>> GetAllAsync()
    {
        var testentitys = await _testentityRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<TestentityReadDto>>(testentitys);
    }

    public async Task<TestentityReadDto?> GetByIdAsync(string id)
    {
        var testentity = await _testentityRepository.GetByIdAsync(id);
        return testentity == null ? null : _mapper.Map<TestentityReadDto>(testentity);
    }

    public async Task<TestentityReadDto> CreateAsync(CreateTestentityDto dto)
    {
        var testentity = _mapper.Map<Testentity>(dto);
        testentity.CreatedAt = DateTime.UtcNow;
        testentity.UpdatedAt = DateTime.UtcNow;
        
        var createdTestentity = await _testentityRepository.CreateAsync(testentity);
        return _mapper.Map<TestentityReadDto>(createdTestentity);
    }

    public async Task<TestentityReadDto?> UpdateAsync(string id, UpdateTestentityDto dto)
    {
        var existingTestentity = await _testentityRepository.GetByIdAsync(id);
        if (existingTestentity == null)
        {
            return null;
        }

        _mapper.Map(dto, existingTestentity);
        existingTestentity.UpdatedAt = DateTime.UtcNow;
        
        var updatedTestentity = await _testentityRepository.UpdateAsync(existingTestentity);
        return updatedTestentity == null ? null : _mapper.Map<TestentityReadDto>(updatedTestentity);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _testentityRepository.DeleteAsync(id);
    }

    public async Task<bool> IsNameAvailableAsync(string name)
    {
        return await _testentityRepository.IsNameAvailableAsync(name);
    }
}