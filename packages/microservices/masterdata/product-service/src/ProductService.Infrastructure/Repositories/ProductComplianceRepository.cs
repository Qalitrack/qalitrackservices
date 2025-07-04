using Microsoft.EntityFrameworkCore;
using ProductService.Core.Entities;
using ProductService.Core.Interfaces;
using ProductService.Infrastructure.Data;

namespace ProductService.Infrastructure.Repositories;

public class ProductComplianceRepository : Repository<ProductCompliance>, IProductComplianceRepository
{
    public ProductComplianceRepository(ProductDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ProductCompliance>> GetByProductIdAsync(string productId)
    {
        return await _dbSet
            .Where(c => c.ProductId == productId && !c.IsDeleted)
            .Include(c => c.Product)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductCompliance>> GetExpiringComplianceAsync(DateTime beforeDate)
    {
        return await _dbSet
            .Where(c => c.ExpiryDate.HasValue && c.ExpiryDate.Value <= beforeDate && !c.IsDeleted)
            .Include(c => c.Product)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductCompliance>> GetByComplianceTypeAsync(string complianceType)
    {
        return await _dbSet
            .Where(c => c.ComplianceType == complianceType && !c.IsDeleted)
            .Include(c => c.Product)
            .ToListAsync();
    }
}