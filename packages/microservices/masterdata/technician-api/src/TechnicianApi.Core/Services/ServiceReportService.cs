using AutoMapper;
using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.DTOs.ServiceReport;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Core.Services;

public class ServiceReportService : IServiceReportService
{
    private readonly IRepository<ServiceReport> _repository;
    private readonly IRepository<Assignment> _assignmentRepository;
    private readonly IRepository<CheckIn> _checkInRepository;
    private readonly IMapper _mapper;

    public ServiceReportService(
        IRepository<ServiceReport> repository,
        IRepository<Assignment> assignmentRepository,
        IRepository<CheckIn> checkInRepository,
        IMapper mapper)
    {
        _repository = repository;
        _assignmentRepository = assignmentRepository;
        _checkInRepository = checkInRepository;
        _mapper = mapper;
    }

    public async Task<ServiceReportResponseDto?> GetByIdAsync(string id)
    {
        var report = await _repository.GetByIdAsync(id);
        return report == null ? null : _mapper.Map<ServiceReportResponseDto>(report);
    }

    public async Task<ServiceReportResponseDto?> GetByAssignmentIdAsync(string assignmentId)
    {
        var report = await _repository.FirstOrDefaultAsync(r => r.AssignmentId == assignmentId);
        return report == null ? null : _mapper.Map<ServiceReportResponseDto>(report);
    }

    public async Task<PagedResponseDto<ServiceReportResponseDto>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? technicianId = null,
        string? status = null)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            pageNumber,
            pageSize,
            filter: report =>
                (string.IsNullOrEmpty(technicianId) || report.TechnicianId == technicianId) &&
                (string.IsNullOrEmpty(status) || report.Status.ToString() == status),
            orderBy: r => r.SubmittedAt!,
            ascending: false
        );

        return new PagedResponseDto<ServiceReportResponseDto>
        {
            Items = _mapper.Map<IEnumerable<ServiceReportResponseDto>>(items),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ServiceReportResponseDto> CreateAsync(CreateServiceReportDto dto)
    {
        var report = _mapper.Map<ServiceReport>(dto);
        var created = await _repository.CreateAsync(report);
        return _mapper.Map<ServiceReportResponseDto>(created);
    }

    public async Task<ServiceReportResponseDto?> UpdateAsync(string id, UpdateServiceReportDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return null;

        _mapper.Map(dto, existing);
        var updated = await _repository.UpdateAsync(existing);
        return updated == null ? null : _mapper.Map<ServiceReportResponseDto>(updated);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<ServiceReportResponseDto?> SubmitReportAsync(string id)
    {
        var report = await _repository.GetByIdAsync(id);
        if (report == null) return null;

        if (report.Status != ServiceReportStatus.Draft)
            return null;

        // Calculate field job time before submission
        await CalculateFieldJobTimeAsync(id);

        report.Status = ServiceReportStatus.Submitted;
        report.SubmittedAt = DateTime.UtcNow;

        // Mark assignment as completed
        var assignment = await _assignmentRepository.GetByIdAsync(report.AssignmentId);
        if (assignment != null)
        {
            assignment.Status = AssignmentStatus.Completed;
            assignment.CompletedAt = DateTime.UtcNow;
            await _assignmentRepository.UpdateAsync(assignment);
        }

        var updated = await _repository.UpdateAsync(report);
        return updated == null ? null : _mapper.Map<ServiceReportResponseDto>(updated);
    }

    public async Task<ServiceReportResponseDto?> ApproveReportAsync(string id, string approvedBy)
    {
        var report = await _repository.GetByIdAsync(id);
        if (report == null) return null;

        if (report.Status != ServiceReportStatus.Submitted && report.Status != ServiceReportStatus.UnderReview)
            return null;

        report.Status = ServiceReportStatus.Approved;
        report.ApprovedAt = DateTime.UtcNow;
        report.ApprovedBy = approvedBy;

        var updated = await _repository.UpdateAsync(report);
        return updated == null ? null : _mapper.Map<ServiceReportResponseDto>(updated);
    }

    public async Task<ServiceReportResponseDto?> RejectReportAsync(string id, string rejectionReason)
    {
        var report = await _repository.GetByIdAsync(id);
        if (report == null) return null;

        if (report.Status != ServiceReportStatus.Submitted && report.Status != ServiceReportStatus.UnderReview)
            return null;

        report.Status = ServiceReportStatus.Rejected;
        report.RejectionReason = rejectionReason;

        var updated = await _repository.UpdateAsync(report);
        return updated == null ? null : _mapper.Map<ServiceReportResponseDto>(updated);
    }

    public async Task<ServiceReportResponseDto> AutoPopulateFromAssignmentAsync(string assignmentId)
    {
        var assignment = await _assignmentRepository.GetByIdWithIncludesAsync(
            assignmentId,
            a => a.Technicians);

        if (assignment == null)
        {
            throw new InvalidOperationException($"Assignment with ID {assignmentId} not found");
        }

        var technician = assignment.Technicians.FirstOrDefault();
        if (technician == null)
        {
            throw new InvalidOperationException($"Assignment {assignmentId} has no assigned technician");
        }

        var serviceReport = new ServiceReport
        {
            Id = Guid.NewGuid().ToString(),
            AssignmentId = assignmentId,
            TechnicianId = technician.Id,
            TechnicianName = technician.Name,
            CustomerName = string.Empty, // To be filled by technician
            LocationName = assignment.LocationName,
            LocationAddress = assignment.LocationAddress,
            ContactPerson = string.Empty, // To be filled by technician
            Designation = string.Empty, // To be filled by technician
            MobileNumber = string.Empty, // To be filled by technician
            Status = ServiceReportStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(serviceReport);

        // Calculate field job time from check-ins
        await CalculateFieldJobTimeAsync(created.Id);

        return _mapper.Map<ServiceReportResponseDto>(created);
    }

    public async Task CalculateFieldJobTimeAsync(string serviceReportId)
    {
        var report = await _repository.GetByIdAsync(serviceReportId);
        if (report == null) return;

        // Get all check-ins for this assignment
        var checkIns = await _checkInRepository.FindAsync(c => c.AssignmentId == report.AssignmentId);
        var checkInList = checkIns.OrderBy(c => c.CheckInTime).ToList();

        if (!checkInList.Any()) return;

        // Calculate total field job time from all check-ins
        int totalMinutes = 0;
        DateTime? earliestCheckIn = null;
        DateTime? latestCheckOut = null;

        foreach (var checkIn in checkInList)
        {
            if (earliestCheckIn == null || checkIn.CheckInTime < earliestCheckIn)
            {
                earliestCheckIn = checkIn.CheckInTime;
            }

            if (checkIn.CheckOutTime.HasValue)
            {
                if (latestCheckOut == null || checkIn.CheckOutTime > latestCheckOut)
                {
                    latestCheckOut = checkIn.CheckOutTime;
                }

                // Calculate duration for this check-in/check-out pair
                var duration = (checkIn.CheckOutTime.Value - checkIn.CheckInTime).TotalMinutes;
                totalMinutes += (int)duration;
            }
        }

        // Update service report with calculated values
        report.StartDay = earliestCheckIn?.Date;
        report.EndDay = latestCheckOut?.Date ?? earliestCheckIn?.Date;
        report.TotalFieldJobMinutes = totalMinutes;

        await _repository.UpdateAsync(report);
    }
}
