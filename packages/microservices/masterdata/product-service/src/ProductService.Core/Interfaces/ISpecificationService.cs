using ProductService.Core.DTOs;
using ProductService.Core.Entities;

namespace ProductService.Core.Interfaces;

public interface ISpecificationService
{
    Task<IEnumerable<SpecificationReadDto>> GetAllAsync();
    Task<SpecificationReadDto?> GetByIdAsync(string id);
    Task<SpecificationReadDto> CreateAsync(CreateSpecificationDto dto);
    Task<SpecificationReadDto?> UpdateAsync(string id, UpdateSpecificationDto dto);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<SpecificationReadDto>> GetByProductIdAsync(string productId);
    Task<IEnumerable<SpecificationReadDto>> GetComplianceSpecificationsAsync(string productId);
    Task<bool> ValidateComplianceAsync(string productId);
    Task<IEnumerable<SpecificationReadDto>> GetExpiredCertificationsAsync();
    Task<IEnumerable<SpecificationReadDto>> GetTestsDueAsync();
}