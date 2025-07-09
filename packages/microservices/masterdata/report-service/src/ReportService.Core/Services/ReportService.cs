using AutoMapper;
using ReportService.Core.DTOs;
using ReportService.Core.Entities;
using ReportService.Core.Interfaces;

namespace ReportService.Core.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly IMapper _mapper;

    public ReportService(IReportRepository reportRepository, IMapper mapper)
    {
        _reportRepository = reportRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ReportReadDto>> GetAllAsync()
    {
        var reports = await _reportRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ReportReadDto>>(reports);
    }

    public async Task<ReportReadDto?> GetByIdAsync(string id)
    {
        var report = await _reportRepository.GetByIdAsync(id);
        return report == null ? null : _mapper.Map<ReportReadDto>(report);
    }

    public async Task<ReportReadDto> CreateAsync(CreateReportDto dto)
    {
        var report = _mapper.Map<Report>(dto);
        report.CreatedAt = DateTime.UtcNow;
        report.UpdatedAt = DateTime.UtcNow;
        
        var createdReport = await _reportRepository.CreateAsync(report);
        return _mapper.Map<ReportReadDto>(createdReport);
    }

    public async Task<ReportReadDto?> UpdateAsync(string id, UpdateReportDto dto)
    {
        var existingReport = await _reportRepository.GetByIdAsync(id);
        if (existingReport == null)
        {
            return null;
        }

        _mapper.Map(dto, existingReport);
        existingReport.UpdatedAt = DateTime.UtcNow;
        
        var updatedReport = await _reportRepository.UpdateAsync(existingReport);
        return updatedReport == null ? null : _mapper.Map<ReportReadDto>(updatedReport);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _reportRepository.DeleteAsync(id);
    }

    public async Task<bool> IsNameAvailableAsync(string name)
    {
        return await _reportRepository.IsNameAvailableAsync(name);
    }
}