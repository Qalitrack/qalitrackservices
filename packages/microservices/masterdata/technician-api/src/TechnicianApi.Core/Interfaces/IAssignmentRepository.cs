using TechnicianApi.Core.Entities;

namespace TechnicianApi.Core.Interfaces;

public interface IAssignmentRepository : IRepository<Assignment>
{
    Task<Assignment?> GetByIdWithTechniciansAsync(string id);
    Task<IEnumerable<Assignment>> GetByTechnicianIdAsync(string technicianId);
    Task<IEnumerable<Assignment>> GetByManagerIdWithTechniciansAsync(string managerId);
    Task<(IEnumerable<Assignment> Items, int TotalCount)> GetPagedWithTechniciansAsync(
        int pageNumber,
        int pageSize,
        string? technicianId = null,
        string? status = null);
    Task<Assignment> CreateWithTechniciansAsync(Assignment assignment, List<string> technicianIds);
    Task<Assignment?> UpdateWithTechniciansAsync(Assignment assignment, List<string>? technicianIds = null);
    Task<Assignment?> AssignTechnicianAsync(string assignmentId, string technicianId);
    Task<Assignment?> UnassignTechnicianAsync(string assignmentId, string technicianId);
}
