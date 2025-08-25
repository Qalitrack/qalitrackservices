using AutoMapper;
using SupplierService.Core.DTOs;
using SupplierService.Core.Entities;
using SupplierService.Core.Interfaces;

namespace SupplierService.Core.Services;

public class PerformanceService : IPerformanceService
{
    private readonly ISupplierPerformanceRepository _performanceRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IMapper _mapper;

    public PerformanceService(
        ISupplierPerformanceRepository performanceRepository,
        ISupplierRepository supplierRepository,
        IMapper mapper)
    {
        _performanceRepository = performanceRepository;
        _supplierRepository = supplierRepository;
        _mapper = mapper;
    }

    public async Task<SupplierPerformanceDto> CreatePerformanceAsync(string supplierId, CreateSupplierPerformanceRequest request)
    {
        // Validate supplier exists
        var supplier = await _supplierRepository.GetByIdAsync(supplierId);
        if (supplier == null)
            throw new KeyNotFoundException($"Supplier with ID '{supplierId}' not found");

        var performance = _mapper.Map<SupplierPerformance>(request);
        performance.SupplierId = supplierId;
        performance.Year = request.EvaluationDate.Year;
        performance.Month = request.EvaluationDate.Month;
        performance.EvaluationDate = request.EvaluationDate;

        // Calculate overall rating from individual scores
        var scores = new[] { request.QualityScore, request.DeliveryScore, request.ServiceScore };
        performance.QualityRating = request.QualityScore;
        performance.DeliveryRating = request.DeliveryScore;
        performance.ServiceRating = request.ServiceScore;
        performance.OverallRating = request.OverallScore;

        var createdPerformance = await _performanceRepository.AddAsync(performance);
        return _mapper.Map<SupplierPerformanceDto>(createdPerformance);
    }

    public async Task<SupplierPerformanceDto> UpdatePerformanceAsync(string id, CreateSupplierPerformanceRequest request)
    {
        var performance = await _performanceRepository.GetByIdAsync(id);
        if (performance == null)
            throw new KeyNotFoundException($"Performance record with ID '{id}' not found");

        _mapper.Map(request, performance);
        performance.UpdatedAt = DateTime.UtcNow;

        // Recalculate ratings
        performance.QualityRating = request.QualityScore;
        performance.DeliveryRating = request.DeliveryScore;
        performance.ServiceRating = request.ServiceScore;
        performance.OverallRating = request.OverallScore;

        var updatedPerformance = await _performanceRepository.UpdateAsync(performance);
        return _mapper.Map<SupplierPerformanceDto>(updatedPerformance);
    }

    public async Task<SupplierPerformanceDto?> GetPerformanceByIdAsync(string id)
    {
        var performance = await _performanceRepository.GetByIdAsync(id);
        return performance == null ? null : _mapper.Map<SupplierPerformanceDto>(performance);
    }

    public async Task<IEnumerable<SupplierPerformanceDto>> GetSupplierPerformanceAsync(string supplierId)
    {
        var performances = await _performanceRepository.GetBySupplierIdAsync(supplierId);
        return _mapper.Map<IEnumerable<SupplierPerformanceDto>>(performances);
    }

    public async Task<IEnumerable<SupplierPerformanceDto>> GetPerformanceByPeriodAsync(int year, int? month = null)
    {
        var performances = await _performanceRepository.GetByPeriodAsync(year, month);
        return _mapper.Map<IEnumerable<SupplierPerformanceDto>>(performances);
    }

    public async Task<decimal?> GetAverageRatingAsync(string supplierId, int? months = null)
    {
        return await _performanceRepository.GetAverageRatingAsync(supplierId, months);
    }

    public async Task<SupplierPerformanceDto?> GetLatestPerformanceAsync(string supplierId)
    {
        var performance = await _performanceRepository.GetLatestPerformanceAsync(supplierId);
        return performance == null ? null : _mapper.Map<SupplierPerformanceDto>(performance);
    }

    public async Task<IEnumerable<SupplierPerformanceDto>> GetTopPerformersAsync(int count = 10)
    {
        var performances = await _performanceRepository.GetTopPerformersAsync(count);
        return _mapper.Map<IEnumerable<SupplierPerformanceDto>>(performances);
    }

    public async Task<IEnumerable<SupplierPerformanceDto>> GetPoorPerformersAsync(decimal threshold = 5.0m)
    {
        var performances = await _performanceRepository.GetPoorPerformersAsync(10);
        return _mapper.Map<IEnumerable<SupplierPerformanceDto>>(performances);
    }

    public async Task DeletePerformanceAsync(string id)
    {
        var performance = await _performanceRepository.GetByIdAsync(id);
        if (performance == null)
            throw new KeyNotFoundException($"Performance record with ID '{id}' not found");

        await _performanceRepository.DeleteAsync(id);
    }

    public async Task<SupplierPerformanceDto> CalculatePerformanceMetricsAsync(string supplierId, int year, int month)
    {
        // This would typically integrate with other systems to calculate actual metrics
        // For now, we'll create a basic implementation
        
        var supplier = await _supplierRepository.GetByIdAsync(supplierId);
        if (supplier == null)
            throw new KeyNotFoundException($"Supplier with ID '{supplierId}' not found");

        // Check if performance already exists for this period
        var existing = await _performanceRepository.GetBySupplierAndPeriodAsync(supplierId, year, month);
        if (existing != null)
            throw new InvalidOperationException($"Performance record already exists for {month}/{year}");

        // In a real implementation, this would calculate metrics from actual data
        var performance = new SupplierPerformance
        {
            SupplierId = supplierId,
            Year = year,
            Month = month,
            EvaluationDate = DateTime.UtcNow,
            QualityRating = 8.0m, // Default values - would be calculated from actual data
            DeliveryRating = 8.5m,
            ServiceRating = 8.2m,
            OverallRating = 8.23m,
            TotalOrders = 0, // Would be calculated from actual orders
            OnTimeDeliveries = 0,
            LateDeliveries = 0,
            DefectiveDeliveries = 0,
            OnTimeDeliveryRate = 0,
            DefectRate = 0,
            ComplaintCount = 0,
            ResolvedComplaints = 0,
            Comments = "Auto-calculated performance metrics",
            EvaluatedBy = "System"
        };

        var createdPerformance = await _performanceRepository.AddAsync(performance);
        return _mapper.Map<SupplierPerformanceDto>(createdPerformance);
    }

}