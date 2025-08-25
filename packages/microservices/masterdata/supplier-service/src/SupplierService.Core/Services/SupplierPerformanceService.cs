using AutoMapper;
using SupplierService.Core.DTOs;
using SupplierService.Core.Entities;
using SupplierService.Core.Interfaces;

namespace SupplierService.Core.Services;

public class SupplierPerformanceService : ISupplierPerformanceService
{
    private readonly ISupplierPerformanceRepository _performanceRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IMapper _mapper;

    public SupplierPerformanceService(
        ISupplierPerformanceRepository performanceRepository,
        ISupplierRepository supplierRepository,
        IMapper mapper)
    {
        _performanceRepository = performanceRepository;
        _supplierRepository = supplierRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SupplierPerformanceDto>> GetAllAsync(string supplierId)
    {
        var performances = await _performanceRepository.GetBySupplierId(supplierId);
        return _mapper.Map<IEnumerable<SupplierPerformanceDto>>(performances);
    }

    public async Task<SupplierPerformanceDto?> GetByIdAsync(string id)
    {
        var performance = await _performanceRepository.GetByIdAsync(id);
        return performance == null ? null : _mapper.Map<SupplierPerformanceDto>(performance);
    }

    public async Task<SupplierPerformanceDto> CreateAsync(string supplierId, CreateSupplierPerformanceDto dto)
    {
        // Verify supplier exists
        var supplier = await _supplierRepository.GetByIdAsync(supplierId);
        if (supplier == null)
            throw new InvalidOperationException($"Supplier with ID '{supplierId}' not found");

        var performance = _mapper.Map<SupplierPerformance>(dto);
        performance.SupplierId = supplierId;

        var createdPerformance = await _performanceRepository.AddAsync(performance);
        return _mapper.Map<SupplierPerformanceDto>(createdPerformance);
    }

    public async Task<SupplierPerformanceDto?> UpdateAsync(string id, UpdateSupplierPerformanceDto dto)
    {
        var performance = await _performanceRepository.GetByIdAsync(id);
        if (performance == null)
            return null;

        _mapper.Map(dto, performance);
        performance.UpdatedAt = DateTime.UtcNow;

        var updatedPerformance = await _performanceRepository.UpdateAsync(performance);
        return _mapper.Map<SupplierPerformanceDto>(updatedPerformance);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var performance = await _performanceRepository.GetByIdAsync(id);
        if (performance == null)
            return false;

        await _performanceRepository.DeleteAsync(id);
        return true;
    }

    public async Task<IEnumerable<SupplierPerformanceDto>> GetByMetricTypeAsync(string supplierId, PerformanceMetricType metricType)
    {
        var performances = await _performanceRepository.GetByMetricType(supplierId, metricType);
        return _mapper.Map<IEnumerable<SupplierPerformanceDto>>(performances);
    }

    public async Task<IEnumerable<SupplierPerformanceDto>> GetByPeriodAsync(string supplierId, PerformancePeriod period)
    {
        var performances = await _performanceRepository.GetByPeriod(supplierId, period);
        return _mapper.Map<IEnumerable<SupplierPerformanceDto>>(performances);
    }

    public async Task<IEnumerable<SupplierPerformanceDto>> GetByDateRangeAsync(string supplierId, DateTime startDate, DateTime endDate)
    {
        var performances = await _performanceRepository.GetByDateRange(supplierId, startDate, endDate);
        return _mapper.Map<IEnumerable<SupplierPerformanceDto>>(performances);
    }

    public async Task<decimal> GetAverageScoreAsync(string supplierId, PerformanceMetricType metricType)
    {
        return await _performanceRepository.GetAverageScore(supplierId, metricType);
    }

    public async Task<SupplierPerformanceSummaryDto> GetPerformanceSummaryAsync(string supplierId)
    {
        var supplier = await _supplierRepository.GetByIdAsync(supplierId);
        if (supplier == null)
            throw new InvalidOperationException($"Supplier with ID '{supplierId}' not found");

        var performances = await _performanceRepository.GetBySupplierId(supplierId);
        var recentPerformances = performances.Take(5).ToList();

        var summary = new SupplierPerformanceSummaryDto
        {
            SupplierId = supplierId,
            SupplierName = supplier.Name,
            TotalDataPoints = performances.Sum(p => p.DataPoints),
            LastUpdated = performances.Any() ? performances.Max(p => p.UpdatedAt) : DateTime.MinValue,
            RecentMetrics = _mapper.Map<List<SupplierPerformanceDto>>(recentPerformances)
        };

        // Calculate average scores by metric type
        if (performances.Any())
        {
            summary.DeliveryTimeScore = await GetAverageScoreAsync(supplierId, PerformanceMetricType.DeliveryTime);
            summary.QualityScore = await GetAverageScoreAsync(supplierId, PerformanceMetricType.Quality);
            summary.ResponseTimeScore = await GetAverageScoreAsync(supplierId, PerformanceMetricType.ResponseTime);
            summary.ReliabilityScore = await GetAverageScoreAsync(supplierId, PerformanceMetricType.Reliability);
            summary.PriceCompetitivenessScore = await GetAverageScoreAsync(supplierId, PerformanceMetricType.PriceCompetitiveness);
            summary.CommunicationScore = await GetAverageScoreAsync(supplierId, PerformanceMetricType.Communication);

            // Calculate overall score
            var scores = new[] { 
                summary.DeliveryTimeScore, 
                summary.QualityScore, 
                summary.ResponseTimeScore, 
                summary.ReliabilityScore, 
                summary.PriceCompetitivenessScore, 
                summary.CommunicationScore 
            }.Where(s => s > 0);

            summary.OverallScore = scores.Any() ? scores.Average() : 0;

            // Assign performance grade
            summary.PerformanceGrade = summary.OverallScore switch
            {
                >= 90 => "A",
                >= 80 => "B",
                >= 70 => "C",
                >= 60 => "D",
                _ => "F"
            };
        }

        return summary;
    }

    public async Task<SupplierPerformanceDto> CreatePerformanceAsync(string supplierId, CreateSupplierPerformanceRequest request)
    {
        var supplier = await _supplierRepository.GetByIdAsync(supplierId);
        if (supplier == null)
            throw new InvalidOperationException($"Supplier with ID '{supplierId}' not found");

        var performance = _mapper.Map<SupplierPerformance>(request);
        performance.SupplierId = supplierId;

        var createdPerformance = await _performanceRepository.AddAsync(performance);
        return _mapper.Map<SupplierPerformanceDto>(createdPerformance);
    }

    public async Task<IEnumerable<SupplierPerformanceDto>> GetSupplierPerformanceAsync(string supplierId)
    {
        var performances = await _performanceRepository.GetBySupplierId(supplierId);
        return _mapper.Map<IEnumerable<SupplierPerformanceDto>>(performances);
    }

    public async Task<decimal?> GetAverageRatingAsync(string supplierId, int? months = null)
    {
        var performances = await _performanceRepository.GetBySupplierId(supplierId);
        
        if (months.HasValue)
        {
            var cutoffDate = DateTime.UtcNow.AddMonths(-months.Value);
            performances = performances.Where(p => p.CreatedAt >= cutoffDate);
        }

        if (!performances.Any())
            return null;

        return performances.Average(p => p.OverallRating ?? 0);
    }

    // Additional methods expected by controllers
    public async Task<IEnumerable<SupplierPerformanceDto>> GetAllAsync()
    {
        var performances = await _performanceRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<SupplierPerformanceDto>>(performances);
    }

    public async Task<SupplierPerformanceDto> CreateAsync(CreateSupplierPerformanceDto dto)
    {
        var performance = _mapper.Map<SupplierPerformance>(dto);
        var createdPerformance = await _performanceRepository.AddAsync(performance);
        return _mapper.Map<SupplierPerformanceDto>(createdPerformance);
    }
}