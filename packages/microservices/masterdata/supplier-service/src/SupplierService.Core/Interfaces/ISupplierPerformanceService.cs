using SupplierService.Core.DTOs;

namespace SupplierService.Core.Interfaces;

public interface ISupplierPerformanceService
{
    Task<SupplierPerformanceDto> CreatePerformanceAsync(string supplierId, CreateSupplierPerformanceRequest request);
    Task<IEnumerable<SupplierPerformanceDto>> GetSupplierPerformanceAsync(string supplierId);
    Task<decimal?> GetAverageRatingAsync(string supplierId, int? months = null);
    Task<SupplierPerformanceSummaryDto> GetPerformanceSummaryAsync(string supplierId);
    
    // Additional methods expected by controllers
    Task<IEnumerable<SupplierPerformanceDto>> GetAllAsync();
    Task<SupplierPerformanceDto> CreateAsync(CreateSupplierPerformanceDto dto);
}