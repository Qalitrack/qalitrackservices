using TechnicianApi.Core.Entities;

namespace TechnicianApi.Core.Interfaces;

public interface IAssignmentRepository : IRepository<Assignment>
{
    Task<Assignment?> GetByIdWithTechnicianIdsAsync(string id);
    Task<IEnumerable<Assignment>> GetByTechnicianIdAsync(string technicianId);
    Task<IEnumerable<Assignment>> GetByManagerIdAsync(string managerId);
    Task<(IEnumerable<Assignment> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? technicianId = null,
        string? status = null);
    Task<Assignment> CreateWithTechnicianIdsAsync(Assignment assignment, List<string> technicianIds);
    Task<Assignment?> UpdateWithTechnicianIdsAsync(Assignment assignment, List<string>? technicianIds = null);
    Task<Assignment?> AssignTechnicianAsync(string assignmentId, string technicianId);
    Task<Assignment?> UnassignTechnicianAsync(string assignmentId, string technicianId);
}
