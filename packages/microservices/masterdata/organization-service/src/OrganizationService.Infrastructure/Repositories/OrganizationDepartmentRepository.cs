using Microsoft.EntityFrameworkCore;
using OrganizationService.Core.Entities;
using OrganizationService.Core.Interfaces;
using OrganizationService.Infrastructure.Data;

namespace OrganizationService.Infrastructure.Repositories;

public class OrganizationDepartmentRepository : Repository<OrganizationDepartment>, IOrganizationDepartmentRepository
{
    public OrganizationDepartmentRepository(OrganizationDbContext context) : base(context) { }

    public async Task<List<OrganizationDepartment>> GetByOrganizationIdAsync(string organizationId)
    {
        return await _dbSet
            .Include(od => od.Manager)
            .Include(od => od.ParentDepartment)
            .Include(od => od.ChildDepartments)
            .Where(od => od.OrganizationId == organizationId)
            .OrderBy(od => od.Name)
            .ToListAsync();
    }

    public async Task<OrganizationDepartment?> GetByCodeAsync(string organizationId, string code)
    {
        return await _dbSet
            .Include(od => od.Manager)
            .Include(od => od.ParentDepartment)
            .Include(od => od.ChildDepartments)
            .FirstOrDefaultAsync(od => od.OrganizationId == organizationId && od.Code == code);
    }

    public async Task<List<OrganizationDepartment>> GetChildDepartmentsAsync(string parentDepartmentId)
    {
        return await _dbSet
            .Include(od => od.Manager)
            .Include(od => od.ChildDepartments)
            .Where(od => od.ParentDepartmentId == parentDepartmentId)
            .OrderBy(od => od.Name)
            .ToListAsync();
    }
}