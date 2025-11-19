using AutoMapper;
using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.DTOs.PerformanceMetrics;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class PerformanceMetricsService : IPerformanceMetricsService
{
    private readonly IRepository<PerformanceMetrics> _repository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IRepository<ServiceReport> _reportRepository;
    private readonly IRepository<Requisition> _requisitionRepository;
    private readonly IRepository<CheckIn> _checkInRepository;
    private readonly IMapper _mapper;

    public PerformanceMetricsService(
        IRepository<PerformanceMetrics> repository,
        IAssignmentRepository assignmentRepository,
        IRepository<ServiceReport> reportRepository,
        IRepository<Requisition> requisitionRepository,
        IRepository<CheckIn> checkInRepository,
        IMapper mapper)
    {
        _repository = repository;
        _assignmentRepository = assignmentRepository;
        _reportRepository = reportRepository;
        _requisitionRepository = requisitionRepository;
        _checkInRepository = checkInRepository;
        _mapper = mapper;
    }

    public async Task<PerformanceMetricsResponseDto?> GetByIdAsync(string id)
    {
        var metrics = await _repository.GetByIdAsync(id);
        return metrics == null ? null : _mapper.Map<PerformanceMetricsResponseDto>(metrics);
    }

    public async Task<PerformanceMetricsResponseDto?> GetLatestByTechnicianIdAsync(string technicianId)
    {
        var allMetrics = await _repository.FindAsync(m => m.TechnicianId == technicianId);
        var latest = allMetrics.OrderByDescending(m => m.CalculatedAt).FirstOrDefault();
        return latest == null ? null : _mapper.Map<PerformanceMetricsResponseDto>(latest);
    }

    public async Task<PagedResponseDto<PerformanceMetricsResponseDto>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? technicianId = null)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            pageNumber,
            pageSize,
            filter: m => string.IsNullOrEmpty(technicianId) || m.TechnicianId == technicianId,
            orderBy: m => m.CalculatedAt,
            ascending: false
        );

        return new PagedResponseDto<PerformanceMetricsResponseDto>
        {
            Items = _mapper.Map<IEnumerable<PerformanceMetricsResponseDto>>(items),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PerformanceMetricsResponseDto> CalculateMetricsAsync(
        string technicianId,
        DateTime periodStart,
        DateTime periodEnd)
    {
        // Get assignments for the technician for the period
        var assignments = await _assignmentRepository.GetByTechnicianIdAsync(technicianId);
        var assignmentList = assignments
            .Where(a => a.CreatedAt >= periodStart && a.CreatedAt <= periodEnd)
            .ToList();

        // Get reports for the period
        var reports = await _reportRepository.FindAsync(r =>
            r.TechnicianId == technicianId &&
            r.CreatedAt >= periodStart &&
            r.CreatedAt <= periodEnd);

        var reportList = reports.ToList();

        // Get requisitions for the period
        var requisitions = await _requisitionRepository.FindAsync(r =>
            r.TechnicianId == technicianId &&
            r.CreatedAt >= periodStart &&
            r.CreatedAt <= periodEnd);

        var requisitionList = requisitions.ToList();

        // Get check-ins for the period
        var checkIns = await _checkInRepository.FindAsync(c =>
            c.CheckInTime >= periodStart &&
            c.CheckInTime <= periodEnd);

        var checkInList = checkIns.ToList();

        // Calculate assignment metrics
        var totalAssignments = assignmentList.Count;
        var completedAssignments = assignmentList.Count(a => a.Status == AssignmentStatus.Completed);
        var delayedAssignments = assignmentList.Count(a =>
            a.Deadline.HasValue &&
            a.CompletedAt.HasValue &&
            a.CompletedAt > a.Deadline);

        var completionRate = totalAssignments > 0
            ? ((decimal)completedAssignments / totalAssignments) * 100
            : 0;

        var onTimeRate = completedAssignments > 0
            ? ((decimal)(completedAssignments - delayedAssignments) / completedAssignments) * 100
            : 0;

        // Calculate average completion time
        var completedWithDuration = assignmentList
            .Where(a => a.StartedAt.HasValue && a.CompletedAt.HasValue)
            .ToList();

        var averageCompletionMinutes = completedWithDuration.Any()
            ? (int)completedWithDuration.Average(a => (a.CompletedAt!.Value - a.StartedAt!.Value).TotalMinutes)
            : 0;

        // Calculate working hours
        var totalWorkingHours = checkInList.Any()
            ? (int)(checkInList.Max(c => c.CheckInTime) - checkInList.Min(c => c.CheckInTime)).TotalHours
            : 0;

        // Calculate report metrics
        var reportsSubmitted = reportList.Count;
        var reportsApproved = reportList.Count(r => r.Status == ServiceReportStatus.Approved);
        var reportsRejected = reportList.Count(r => r.Status == ServiceReportStatus.Rejected);
        var reportApprovalRate = reportsSubmitted > 0
            ? ((decimal)reportsApproved / reportsSubmitted) * 100
            : 0;

        // Calculate requisition metrics
        var totalRequisitions = requisitionList.Count;
        var approvedRequisitions = requisitionList.Count(r => r.Status == RequisitionStatus.Paid || r.Status == RequisitionStatus.CfoApproved);
        var totalRequisitionAmount = requisitionList.Sum(r => r.Amount);

        // Calculate overall performance score
        var performanceScore = (completionRate * 0.4m) + (onTimeRate * 0.3m) + (reportApprovalRate * 0.3m);

        // Determine alert level
        var alertLevel = PerformanceAlertLevel.None;
        if (delayedAssignments >= 4) alertLevel = PerformanceAlertLevel.Red;
        else if (delayedAssignments >= 2) alertLevel = PerformanceAlertLevel.Amber;

        // Create metrics entity
        var metrics = new PerformanceMetrics
        {
            TechnicianId = technicianId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            TotalAssignments = totalAssignments,
            CompletedAssignments = completedAssignments,
            DelayedAssignments = delayedAssignments,
            CompletionRate = completionRate,
            OnTimeRate = onTimeRate,
            PerformanceScore = performanceScore,
            AlertLevel = alertLevel,
            AverageCompletionMinutes = averageCompletionMinutes,
            TotalWorkingHours = totalWorkingHours,
            ReportsSubmitted = reportsSubmitted,
            ReportsApproved = reportsApproved,
            ReportsRejected = reportsRejected,
            ReportApprovalRate = reportApprovalRate,
            TotalRequisitions = totalRequisitions,
            ApprovedRequisitions = approvedRequisitions,
            TotalRequisitionAmount = totalRequisitionAmount,
            AmberThreshold = 2,
            RedThreshold = 4,
            CalculatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(metrics);
        return _mapper.Map<PerformanceMetricsResponseDto>(created);
    }
}
