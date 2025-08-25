using AutoMapper;
using ProductService.Core.DTOs;
using ProductService.Core.Entities;
using ProductService.Core.Interfaces;

namespace ProductService.Core.Services;

public class SpecificationService : ISpecificationService
{
    private readonly ISpecificationRepository _specificationRepository;
    private readonly IMapper _mapper;

    public SpecificationService(ISpecificationRepository specificationRepository, IMapper mapper)
    {
        _specificationRepository = specificationRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SpecificationReadDto>> GetAllAsync()
    {
        var specifications = await _specificationRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<SpecificationReadDto>>(specifications);
    }

    public async Task<SpecificationReadDto?> GetByIdAsync(string id)
    {
        var specification = await _specificationRepository.GetByIdAsync(id);
        return specification == null ? null : _mapper.Map<SpecificationReadDto>(specification);
    }

    public async Task<SpecificationReadDto> CreateAsync(CreateSpecificationDto dto)
    {
        var specification = _mapper.Map<Specification>(dto);
        specification.CreatedAt = DateTime.UtcNow;
        specification.UpdatedAt = DateTime.UtcNow;
        
        // Parse numeric value if applicable
        if (decimal.TryParse(specification.Value, out var numericValue))
        {
            specification.NumericValue = numericValue;
        }
        
        var createdSpecification = await _specificationRepository.CreateAsync(specification);
        return _mapper.Map<SpecificationReadDto>(createdSpecification);
    }

    public async Task<SpecificationReadDto?> UpdateAsync(string id, UpdateSpecificationDto dto)
    {
        var existingSpecification = await _specificationRepository.GetByIdAsync(id);
        if (existingSpecification == null)
        {
            return null;
        }

        _mapper.Map(dto, existingSpecification);
        existingSpecification.UpdatedAt = DateTime.UtcNow;
        
        // Parse numeric value if applicable
        if (decimal.TryParse(existingSpecification.Value, out var numericValue))
        {
            existingSpecification.NumericValue = numericValue;
        }
        
        var updatedSpecification = await _specificationRepository.UpdateAsync(existingSpecification);
        return updatedSpecification == null ? null : _mapper.Map<SpecificationReadDto>(updatedSpecification);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _specificationRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<SpecificationReadDto>> GetByProductIdAsync(string productId)
    {
        var specifications = await _specificationRepository.GetByProductIdAsync(productId);
        return _mapper.Map<IEnumerable<SpecificationReadDto>>(specifications);
    }

    public async Task<IEnumerable<SpecificationReadDto>> GetComplianceSpecificationsAsync(string productId)
    {
        var specifications = await _specificationRepository.GetByProductIdAsync(productId);
        var complianceSpecs = specifications.Where(s => s.IsComplianceRequired).ToList();
        return _mapper.Map<IEnumerable<SpecificationReadDto>>(complianceSpecs);
    }

    public async Task<bool> ValidateComplianceAsync(string productId)
    {
        var specifications = await _specificationRepository.GetByProductIdAsync(productId);
        var complianceSpecs = specifications.Where(s => s.IsComplianceRequired).ToList();
        
        return complianceSpecs.All(spec => spec.IsCompliant && !spec.IsCertificationExpired);
    }

    public async Task<IEnumerable<SpecificationReadDto>> GetExpiredCertificationsAsync()
    {
        var specifications = await _specificationRepository.GetAllAsync();
        var expiredSpecs = specifications.Where(s => 
            s.CertificationExpiry.HasValue && 
            s.CertificationExpiry.Value < DateTime.UtcNow).ToList();
        
        return _mapper.Map<IEnumerable<SpecificationReadDto>>(expiredSpecs);
    }

    public async Task<IEnumerable<SpecificationReadDto>> GetTestsDueAsync()
    {
        var specifications = await _specificationRepository.GetAllAsync();
        var testsDue = specifications.Where(s => 
            s.NextTestDue.HasValue && 
            s.NextTestDue.Value <= DateTime.UtcNow.AddDays(7)).ToList();
        
        return _mapper.Map<IEnumerable<SpecificationReadDto>>(testsDue);
    }
}