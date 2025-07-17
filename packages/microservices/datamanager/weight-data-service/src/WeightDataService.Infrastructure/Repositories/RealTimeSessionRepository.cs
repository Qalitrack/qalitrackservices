using Microsoft.EntityFrameworkCore;
using WeightDataService.Core.Entities;
using WeightDataService.Core.Interfaces;
using WeightDataService.Infrastructure.Data;

namespace WeightDataService.Infrastructure.Repositories;

public class RealTimeSessionRepository : Repository<RealTimeSession>, IRealTimeSessionRepository
{
    public RealTimeSessionRepository(WeightDataContext context) : base(context)
    {
    }

    public async Task<List<RealTimeSession>> GetActiveSessionsAsync(string organizationId)
    {
        return await _context.Set<RealTimeSession>()
            .Where(s => s.OrganizationId == organizationId && s.Status == StreamingStatus.Active)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<RealTimeSession?> GetSessionByIdAsync(string sessionId, string organizationId)
    {
        return await _context.Set<RealTimeSession>()
            .Include(s => s.Measurements)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.OrganizationId == organizationId);
    }

    public async Task<List<RealTimeSession>> GetSessionsByWeighbridgeAsync(string weighbridgeId, string organizationId)
    {
        return await _context.Set<RealTimeSession>()
            .Where(s => s.WeighbridgeId == weighbridgeId && s.OrganizationId == organizationId)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<bool> UpdateSessionStatusAsync(string sessionId, StreamingStatus status)
    {
        var session = await _context.Set<RealTimeSession>()
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

        if (session == null) return false;

        session.Status = status;
        session.UpdatedAt = DateTime.UtcNow;

        if (status == StreamingStatus.Completed || status == StreamingStatus.Cancelled)
        {
            session.EndTime = DateTime.UtcNow;
        }

        return await _context.SaveChangesAsync() > 0;
    }
}