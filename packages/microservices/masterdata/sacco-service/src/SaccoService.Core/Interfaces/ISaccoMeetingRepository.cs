using SaccoService.Core.Entities;

namespace SaccoService.Core.Interfaces;

public interface ISaccoMeetingRepository : IRepository<SaccoMeeting>
{
    Task<IEnumerable<SaccoMeeting>> GetMeetingsBySaccoAsync(string saccoId);
    Task<IEnumerable<SaccoMeeting>> GetUpcomingMeetingsAsync(string saccoId);
    Task<IEnumerable<SaccoMeeting>> GetMeetingsByTypeAsync(string saccoId, MeetingType type);
    Task<IEnumerable<SaccoMeeting>> GetMeetingsByStatusAsync(string saccoId, MeetingStatus status);
    Task<SaccoMeeting?> GetMeetingWithDetailsAsync(string meetingId);
}