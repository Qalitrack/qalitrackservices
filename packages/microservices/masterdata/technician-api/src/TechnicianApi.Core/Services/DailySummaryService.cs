using AutoMapper;
using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.DTOs.DailySummary;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class DailySummaryService : IDailySummaryService
{
    private readonly IRepository<DailySummary> _repository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IRepository<CheckIn> _checkInRepository;
    private readonly IRepository<Requisition> _requisitionRepository;
    private readonly IMapper _mapper;

    public DailySummaryService(
        IRepository<DailySummary> repository,
        IAssignmentRepository assignmentRepository,
        IRepository<CheckIn> checkInRepository,
        IRepository<Requisition> requisitionRepository,
        IMapper mapper)
    {
        _repository = repository;
        _assignmentRepository = assignmentRepository;
        _checkInRepository = checkInRepository;
        _requisitionRepository = requisitionRepository;
        _mapper = mapper;
    }

    public async Task<DailySummaryResponseDto?> GetByIdAsync(string id)
    {
        var summary = await _repository.GetByIdAsync(id);
        return summary == null ? null : _mapper.Map<DailySummaryResponseDto>(summary);
    }

    public async Task<DailySummaryResponseDto?> GetByTechnicianAndDateAsync(string technicianId, DateTime date)
    {
        var dateOnly = date.Date;
        var summary = await _repository.FirstOrDefaultAsync(s =>
            s.TechnicianId == technicianId && s.Date.Date == dateOnly);
        return summary == null ? null : _mapper.Map<DailySummaryResponseDto>(summary);
    }

    public async Task<PagedResponseDto<DailySummaryResponseDto>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? technicianId = null)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            pageNumber,
            pageSize,
            filter: s => string.IsNullOrEmpty(technicianId) || s.TechnicianId == technicianId,
            orderBy: s => s.Date,
            ascending: false
        );

        return new PagedResponseDto<DailySummaryResponseDto>
        {
            Items = _mapper.Map<IEnumerable<DailySummaryResponseDto>>(items),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<DailySummaryResponseDto> GenerateSummaryAsync(string technicianId, DateTime date)
    {
        var dateOnly = date.Date;
        var nextDay = dateOnly.AddDays(1);

        // Get assignments for the technician for the day
        var assignments = await _assignmentRepository.GetByTechnicianIdAsync(technicianId);
        var assignmentList = assignments
            .Where(a => a.CreatedAt >= dateOnly && a.CreatedAt < nextDay)
            .ToList();

        // Get check-ins for the day
        var checkIns = await _checkInRepository.FindAsync(c =>
            c.CheckInTime >= dateOnly &&
            c.CheckInTime < nextDay);

        var checkInList = checkIns.ToList();

        // Get requisitions for the day
        var requisitions = await _requisitionRepository.FindAsync(r =>
            r.TechnicianId == technicianId &&
            r.CreatedAt >= dateOnly &&
            r.CreatedAt < nextDay);

        var requisitionList = requisitions.ToList();

        // Calculate metrics
        var totalAssignments = assignmentList.Count;
        var completedTasks = assignmentList.Count(a => a.Status == AssignmentStatus.Completed);
        var pendingTasks = assignmentList.Count(a => a.Status == AssignmentStatus.Pending || a.Status == AssignmentStatus.Accepted);
        var delayedTasks = assignmentList.Count(a => a.Deadline.HasValue && a.Deadline < DateTime.UtcNow && a.Status != AssignmentStatus.Completed);

        // Calculate alert level
        var alertLevel = PerformanceAlertLevel.None;
        if (delayedTasks >= 4) alertLevel = PerformanceAlertLevel.Red;
        else if (delayedTasks >= 2) alertLevel = PerformanceAlertLevel.Amber;

        // Calculate performance score
        var performanceScore = totalAssignments > 0
            ? ((decimal)completedTasks / totalAssignments) * 100
            : 0;

        // Calculate working time
        var firstCheckIn = checkInList.MinBy(c => c.CheckInTime)?.CheckInTime;
        var lastCheckOut = checkInList.MaxBy(c => c.CheckInTime)?.CheckInTime;
        var totalWorkingMinutes = firstCheckIn.HasValue && lastCheckOut.HasValue
            ? (int)(lastCheckOut.Value - firstCheckIn.Value).TotalMinutes
            : 0;

        // Check if summary already exists
        var existing = await _repository.FirstOrDefaultAsync(s =>
            s.TechnicianId == technicianId && s.Date.Date == dateOnly);

        DailySummary summary;
        if (existing != null)
        {
            // Update existing
            existing.TotalAssignments = totalAssignments;
            existing.CompletedTasks = completedTasks;
            existing.PendingTasks = pendingTasks;
            existing.DelayedTasks = delayedTasks;
            existing.PerformanceScore = performanceScore;
            existing.AlertLevel = alertLevel;
            existing.TotalWorkingMinutes = totalWorkingMinutes;
            existing.FirstCheckIn = firstCheckIn;
            existing.LastCheckOut = lastCheckOut;
            existing.TotalRequisitions = requisitionList.Count;
            existing.TotalRequisitionAmount = requisitionList.Sum(r => r.Amount);
            existing.AutoGeneratedNotes = $"Summary generated at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

            summary = (await _repository.UpdateAsync(existing))!;
        }
        else
        {
            // Create new
            summary = new DailySummary
            {
                TechnicianId = technicianId,
                Date = dateOnly,
                TotalAssignments = totalAssignments,
                CompletedTasks = completedTasks,
                PendingTasks = pendingTasks,
                DelayedTasks = delayedTasks,
                PerformanceScore = performanceScore,
                AlertLevel = alertLevel,
                TotalWorkingMinutes = totalWorkingMinutes,
                FirstCheckIn = firstCheckIn,
                LastCheckOut = lastCheckOut,
                TotalRequisitions = requisitionList.Count,
                TotalRequisitionAmount = requisitionList.Sum(r => r.Amount),
                AutoGeneratedNotes = $"Summary generated at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC"
            };

            summary = await _repository.CreateAsync(summary);
        }

        return _mapper.Map<DailySummaryResponseDto>(summary);
    }
}
