
using Microsoft.EntityFrameworkCore;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;
using TechnicianApi.Infrastructure.Data;

namespace TechnicianApi.Infrastructure.Repositories;

public class AssignmentRepository : Repository<Assignment>, IAssignmentRepository
{
    public AssignmentRepository(TechnicianApiDbContext context) : base(context)
    {
    }

    public async Task<Assignment?> GetByIdWithTechnicianIdsAsync(string id)
    {
        return await _context.Assignments
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
    }

    public async Task<IEnumerable<Assignment>> GetByTechnicianIdAsync(string technicianId)
    {
        return await _context.Assignments
            .Where(a => !a.IsDeleted && a.TechnicianIds.Contains(technicianId))
            .ToListAsync();
    }

    public async Task<IEnumerable<Assignment>> GetByManagerIdAsync(string managerId)
    {
        return await _context.Assignments
            .Where(a => !a.IsDeleted && a.ManagerId == managerId)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Assignment> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? technicianId = null,
        string? status = null)
    {
        var query = _context.Assignments
            .Where(a => !a.IsDeleted);

        if (!string.IsNullOrEmpty(technicianId))
        {
            query = query.Where(a => a.TechnicianIds.Contains(technicianId));
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(a => a.Status.ToString() == status);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(a => a.Deadline)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Assignment> CreateWithTechnicianIdsAsync(Assignment assignment)
    {
        assignment.Id = Guid.NewGuid().ToString();
        assignment.CreatedAt = DateTime.UtcNow;
        assignment.UpdatedAt = DateTime.UtcNow;
        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync();

        return assignment;
    }

    public async Task<Assignment?> UpdateWithTechnicianIdsAsync(Assignment assignment)
    {
        var existing = await _context.Assignments
            .FirstOrDefaultAsync(a => a.Id == assignment.Id && !a.IsDeleted);

        if (existing == null) return null;

        _context.Entry(existing).CurrentValues.SetValues(assignment);
        
        existing.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return existing;
    }

    public async Task<Assignment?> AssignTechnicianAsync(string assignmentId, string technicianId)
    {
        var assignment = await GetByIdWithTechnicianIdsAsync(assignmentId);
        if (assignment == null) return null;

        // Check if already assigned
        if (assignment.TechnicianIds.Contains(technicianId))
            return assignment;

        assignment.TechnicianIds.Add(technicianId);
        assignment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return assignment;
    }

    public async Task<Assignment?> UnassignTechnicianAsync(string assignmentId, string technicianId)
    {
        var assignment = await GetByIdWithTechnicianIdsAsync(assignmentId);
        if (assignment == null) return null;

        assignment.TechnicianIds.Remove(technicianId);
        assignment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return assignment;
    }
}
 