using DriverService.Core.Entities;
using DriverService.Core.Interfaces;
using DriverService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DriverService.Infrastructure.Repositories;

public class DriverDocumentRepository : Repository<DriverDocument>, IDriverDocumentRepository
{
    public DriverDocumentRepository(DriverDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<DriverDocument>> GetByDriverIdAsync(string driverId)
    {
        return await _dbSet
            .Where(d => d.DriverId == driverId)
            .OrderBy(d => d.DocumentType)
            .ThenByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<DriverDocument>> GetExpiringDocumentsAsync(int daysAhead = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);
        
        return await _dbSet
            .Include(d => d.Driver)
            .Where(d => d.ExpiryDate.HasValue && 
                       d.ExpiryDate.Value <= cutoffDate)
            .OrderBy(d => d.ExpiryDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<DriverDocument>> GetDocumentsByTypeAsync(DocumentType documentType)
    {
        return await _dbSet
            .Include(d => d.Driver)
            .Where(d => d.DocumentType == documentType)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<DriverDocument>> GetUnverifiedDocumentsAsync()
    {
        return await _dbSet
            .Include(d => d.Driver)
            .Where(d => !d.IsVerified)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }
}