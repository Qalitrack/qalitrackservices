using Microsoft.EntityFrameworkCore;
using WeightDataService.Core.Entities;
using WeightDataService.Core.Interfaces;
using WeightDataService.Infrastructure.Data;

namespace WeightDataService.Infrastructure.Repositories;

public class CalibrationRecordRepository : Repository<CalibrationRecord>, ICalibrationRecordRepository
{
    public CalibrationRecordRepository(WeightDataContext context) : base(context)
    {
    }

    public async Task<List<CalibrationRecord>> GetByWeighbridgeIdAsync(string weighbridgeId, string organizationId)
    {
        return await _context.Set<CalibrationRecord>()
            .Where(c => c.WeighbridgeId == weighbridgeId && c.OrganizationId == organizationId)
            .OrderByDescending(c => c.CalibrationDate)
            .ToListAsync();
    }

    public async Task<CalibrationRecord?> GetLatestCalibrationAsync(string weighbridgeId, string organizationId)
    {
        return await _context.Set<CalibrationRecord>()
            .Where(c => c.WeighbridgeId == weighbridgeId && c.OrganizationId == organizationId)
            .OrderByDescending(c => c.CalibrationDate)
            .FirstOrDefaultAsync();
    }

    public async Task<List<CalibrationRecord>> GetCalibrationsDueAsync(string organizationId, DateTime? beforeDate = null)
    {
        var dueDate = beforeDate ?? DateTime.UtcNow.AddDays(7); // Default to 7 days ahead

        return await _context.Set<CalibrationRecord>()
            .Where(c => c.OrganizationId == organizationId && c.NextCalibrationDue <= dueDate)
            .OrderBy(c => c.NextCalibrationDue)
            .ToListAsync();
    }

    public async Task<List<CalibrationRecord>> GetCalibrationHistoryAsync(string weighbridgeId, string organizationId, int limit = 50)
    {
        return await _context.Set<CalibrationRecord>()
            .Where(c => c.WeighbridgeId == weighbridgeId && c.OrganizationId == organizationId)
            .OrderByDescending(c => c.CalibrationDate)
            .Take(limit)
            .ToListAsync();
    }
}