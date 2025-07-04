using Microsoft.EntityFrameworkCore;
using SaccoService.Core.Entities;
using SaccoService.Core.Interfaces;
using SaccoService.Infrastructure.Data;

namespace SaccoService.Infrastructure.Repositories;

public class SaccoMeetingRepository : Repository<SaccoMeeting>, ISaccoMeetingRepository
{
    public SaccoMeetingRepository(SaccoDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SaccoMeeting>> GetMeetingsBySaccoAsync(string saccoId)
    {
        return await _dbSet
            .Include(m => m.Chairperson)
            .Include(m => m.Secretary)
            .Where(m => !m.IsDeleted && m.SaccoId == saccoId)
            .OrderByDescending(m => m.ScheduledDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<SaccoMeeting>> GetUpcomingMeetingsAsync(string saccoId)
    {
        return await _dbSet
            .Include(m => m.Chairperson)
            .Include(m => m.Secretary)
            .Where(m => !m.IsDeleted && 
                   m.SaccoId == saccoId && 
                   m.ScheduledDate > DateTime.UtcNow &&
                   m.Status == MeetingStatus.Scheduled)
            .OrderBy(m => m.ScheduledDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<SaccoMeeting>> GetMeetingsByTypeAsync(string saccoId, MeetingType type)
    {
        return await _dbSet
            .Include(m => m.Chairperson)
            .Include(m => m.Secretary)
            .Where(m => !m.IsDeleted && m.SaccoId == saccoId && m.MeetingType == type)
            .OrderByDescending(m => m.ScheduledDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<SaccoMeeting>> GetMeetingsByStatusAsync(string saccoId, MeetingStatus status)
    {
        return await _dbSet
            .Include(m => m.Chairperson)
            .Include(m => m.Secretary)
            .Where(m => !m.IsDeleted && m.SaccoId == saccoId && m.Status == status)
            .OrderByDescending(m => m.ScheduledDate)
            .ToListAsync();
    }

    public async Task<SaccoMeeting?> GetMeetingWithDetailsAsync(string meetingId)
    {
        return await _dbSet
            .Include(m => m.Chairperson)
            .Include(m => m.Secretary)
            .Include(m => m.Sacco)
            .FirstOrDefaultAsync(m => m.Id == meetingId && !m.IsDeleted);
    }
}