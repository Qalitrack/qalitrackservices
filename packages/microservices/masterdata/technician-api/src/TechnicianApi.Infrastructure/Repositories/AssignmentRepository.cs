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

    public async Task<Assignment?> GetByIdWithTechniciansAsync(string id)
    {
        return await _context.Assignments
            .Include(a => a.Technicians)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
    }

    public async Task<IEnumerable<Assignment>> GetByTechnicianIdAsync(string technicianId)
    {
        return await _context.Assignments
            .Include(a => a.Technicians)
            .Where(a => !a.IsDeleted && a.Technicians.Any(t => t.Id == technicianId))
            .ToListAsync();
    }

    public async Task<IEnumerable<Assignment>> GetByManagerIdWithTechniciansAsync(string managerId)
    {
        return await _context.Assignments
            .Include(a => a.Technicians)
            .Where(a => !a.IsDeleted && a.ManagerId == managerId)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Assignment> Items, int TotalCount)> GetPagedWithTechniciansAsync(
        int pageNumber,
        int pageSize,
        string? technicianId = null,
        string? status = null)
    {
        var query = _context.Assignments
            .Include(a => a.Technicians)
            .Where(a => !a.IsDeleted);

        if (!string.IsNullOrEmpty(technicianId))
        {
            query = query.Where(a => a.Technicians.Any(t => t.Id == technicianId));
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

    public async Task<Assignment> CreateWithTechniciansAsync(Assignment assignment, List<string> technicianIds)
    {
        assignment.Id = Guid.NewGuid().ToString();
        assignment.CreatedAt = DateTime.UtcNow;
        assignment.UpdatedAt = DateTime.UtcNow;

        // Add technicians to the assignment
        if (technicianIds != null && technicianIds.Any())
        {
            foreach (var technicianId in technicianIds)
            {
                var technician = await _context.Technicians.FindAsync(technicianId);
                if (technician != null)
                {
                    assignment.Technicians.Add(technician);
                }
            }
        }

        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync();

        // Reload with technicians to return complete data
        return (await GetByIdWithTechniciansAsync(assignment.Id))!;
    }

    public async Task<Assignment?> UpdateWithTechniciansAsync(Assignment assignment, List<string>? technicianIds = null)
    {
        var existing = await _context.Assignments
            .Include(a => a.Technicians)
            .FirstOrDefaultAsync(a => a.Id == assignment.Id && !a.IsDeleted);

        if (existing == null) return null;

        // Update properties
        _context.Entry(existing).CurrentValues.SetValues(assignment);

        // Update technicians if provided
        if (technicianIds != null)
        {
            existing.Technicians.Clear();
            foreach (var technicianId in technicianIds)
            {
                var technician = await _context.Technicians.FindAsync(technicianId);
                if (technician != null)
                {
                    existing.Technicians.Add(technician);
                }
            }
        }

        existing.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return existing;
    }

    public async Task<Assignment?> AssignTechnicianAsync(string assignmentId, string technicianId)
    {
        var assignment = await GetByIdWithTechniciansAsync(assignmentId);
        if (assignment == null) return null;

        var technician = await _context.Technicians.FindAsync(technicianId);
        if (technician == null) return null;

        // Check if already assigned
        if (assignment.Technicians.Any(t => t.Id == technicianId))
            return assignment;

        assignment.Technicians.Add(technician);
        assignment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return assignment;
    }

    public async Task<Assignment?> UnassignTechnicianAsync(string assignmentId, string technicianId)
    {
        var assignment = await GetByIdWithTechniciansAsync(assignmentId);
        if (assignment == null) return null;

        var technician = assignment.Technicians.FirstOrDefault(t => t.Id == technicianId);
        if (technician == null) return null;

        assignment.Technicians.Remove(technician);
        assignment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return assignment;
    }
}
