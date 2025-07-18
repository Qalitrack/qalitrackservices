using Microsoft.EntityFrameworkCore;
using ProductService.Core.Entities;
using ProductService.Core.Interfaces;
using ProductService.Infrastructure.Data;

namespace ProductService.Infrastructure.Repositories;

public class SpecificationRepository : Repository<Specification>, ISpecificationRepository
{
    public SpecificationRepository(ProductServiceDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Specification>> GetByProductIdAsync(string productId)
    {
        return await _context.Specifications
            .Where(s => s.ProductId == productId)
            .Include(s => s.Product)
            .OrderBy(s => s.Category)
            .ThenBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Specification>> GetByTypeAsync(SpecificationType type)
    {
        return await _context.Specifications
            .Where(s => s.Type == type)
            .Include(s => s.Product)
            .OrderBy(s => s.Product!.Name)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Specification>> GetComplianceSpecificationsAsync(string productId)
    {
        return await _context.Specifications
            .Where(s => s.ProductId == productId && s.IsComplianceRequired)
            .Include(s => s.Product)
            .OrderBy(s => s.ComplianceStandard)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Specification>> GetExpiringCertificationsAsync(int daysAhead = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);
        return await _context.Specifications
            .Where(s => s.CertificationExpiry.HasValue && 
                       s.CertificationExpiry.Value <= cutoffDate &&
                       s.CertificationExpiry.Value > DateTime.UtcNow &&
                       s.IsComplianceRequired)
            .Include(s => s.Product)
            .OrderBy(s => s.CertificationExpiry)
            .ToListAsync();
    }

    public async Task<IEnumerable<Specification>> GetByComplianceStandardAsync(string standard)
    {
        return await _context.Specifications
            .Where(s => s.ComplianceStandard == standard)
            .Include(s => s.Product)
            .OrderBy(s => s.Product!.Name)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Specification>> GetRequiredSpecificationsAsync(string productId)
    {
        return await _context.Specifications
            .Where(s => s.ProductId == productId && s.IsRequired)
            .Include(s => s.Product)
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Specification>> GetByStatusAsync(SpecificationStatus status)
    {
        return await _context.Specifications
            .Where(s => s.Status == status)
            .Include(s => s.Product)
            .OrderBy(s => s.Product!.Name)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<bool> ValidateProductComplianceAsync(string productId)
    {
        var complianceSpecs = await _context.Specifications
            .Where(s => s.ProductId == productId && s.IsComplianceRequired)
            .ToListAsync();

        return complianceSpecs.All(spec => 
            spec.ComplianceStatus == ComplianceStatus.Compliant && 
            (!spec.CertificationExpiry.HasValue || spec.CertificationExpiry.Value > DateTime.UtcNow));
    }

    public override async Task<IEnumerable<Specification>> GetAllAsync()
    {
        return await _context.Specifications
            .Include(s => s.Product)
            .OrderBy(s => s.Product!.Name)
            .ThenBy(s => s.Category)
            .ThenBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    public override async Task<Specification?> GetByIdAsync(string id)
    {
        return await _context.Specifications
            .Include(s => s.Product)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Specification>> GetByCategoryAsync(string category)
    {
        return await _context.Specifications
            .Where(s => s.Category == category)
            .Include(s => s.Product)
            .OrderBy(s => s.Product!.Name)
            .ThenBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Specification>> GetExpiredCertificationsAsync()
    {
        return await _context.Specifications
            .Where(s => s.CertificationExpiry.HasValue && 
                       s.CertificationExpiry.Value < DateTime.UtcNow &&
                       s.IsComplianceRequired)
            .Include(s => s.Product)
            .OrderBy(s => s.CertificationExpiry)
            .ToListAsync();
    }

    public async Task<IEnumerable<Specification>> GetTestsDueAsync()
    {
        return await _context.Specifications
            .Where(s => s.NextTestDue.HasValue && 
                       s.NextTestDue.Value <= DateTime.UtcNow)
            .Include(s => s.Product)
            .OrderBy(s => s.NextTestDue)
            .ToListAsync();
    }

    public async Task<bool> IsProductCompliantAsync(string productId)
    {
        return await ValidateProductComplianceAsync(productId);
    }

    public async Task<Specification?> GetByNameAsync(string productId, string name)
    {
        return await _context.Specifications
            .Where(s => s.ProductId == productId && s.Name == name)
            .Include(s => s.Product)
            .FirstOrDefaultAsync();
    }
}