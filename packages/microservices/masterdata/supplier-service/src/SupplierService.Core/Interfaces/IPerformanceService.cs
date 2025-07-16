using SupplierService.Core.DTOs;

namespace SupplierService.Core.Interfaces;

public interface IPerformanceService
{
    Task<SupplierPerformanceDto> CreatePerformanceAsync(string supplierId, CreateSupplierPerformanceRequest request);
    Task<SupplierPerformanceDto> UpdatePerformanceAsync(string id, CreateSupplierPerformanceRequest request);
    Task<SupplierPerformanceDto?> GetPerformanceByIdAsync(string id);
    Task<IEnumerable<SupplierPerformanceDto>> GetSupplierPerformanceAsync(string supplierId);
    Task<IEnumerable<SupplierPerformanceDto>> GetPerformanceByPeriodAsync(int year, int? month = null);
    Task<decimal?> GetAverageRatingAsync(string supplierId, int? months = null);
    Task<SupplierPerformanceDto?> GetLatestPerformanceAsync(string supplierId);
    Task<IEnumerable<SupplierPerformanceDto>> GetTopPerformersAsync(int count = 10);
    Task<IEnumerable<SupplierPerformanceDto>> GetPoorPerformersAsync(decimal threshold = 5.0m);
    Task DeletePerformanceAsync(string id);
    Task<SupplierPerformanceDto> CalculatePerformanceMetricsAsync(string supplierId, int year, int month);
}