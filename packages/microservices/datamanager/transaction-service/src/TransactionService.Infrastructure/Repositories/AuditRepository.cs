using Microsoft.EntityFrameworkCore;
using TransactionService.Core.Entities;
using TransactionService.Core.Interfaces;
using TransactionService.Infrastructure.Data;

namespace TransactionService.Infrastructure.Repositories;

public class AuditRepository : Repository<TransactionAudit>, IAuditRepository
{
    public AuditRepository(TransactionDbContext context) : base(context)
    {
    }

    public async Task<List<TransactionAudit>> GetTransactionAuditTrailAsync(string transactionId)
    {
        return await _dbSet
            .Where(a => a.TransactionId == transactionId && !a.IsDeleted)
            .OrderBy(a => a.AuditDate)
            .ToListAsync();
    }

    public async Task<List<TransactionAudit>> GetAuditsByUserAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _dbSet.Where(a => a.UserId == userId && !a.IsDeleted);

        if (fromDate.HasValue)
        {
            query = query.Where(a => a.AuditDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(a => a.AuditDate <= toDate.Value);
        }

        return await query
            .OrderByDescending(a => a.AuditDate)
            .ToListAsync();
    }

    public async Task<List<TransactionAudit>> GetAuditsByActionAsync(AuditAction action, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _dbSet.Where(a => a.AuditAction == action && !a.IsDeleted);

        if (fromDate.HasValue)
        {
            query = query.Where(a => a.AuditDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(a => a.AuditDate <= toDate.Value);
        }

        return await query
            .OrderByDescending(a => a.AuditDate)
            .ToListAsync();
    }

    public async Task<List<TransactionAudit>> GetAuditsByEntityAsync(string entityType, string entityId)
    {
        return await _dbSet
            .Where(a => a.EntityType == entityType && a.EntityId == entityId && !a.IsDeleted)
            .OrderByDescending(a => a.AuditDate)
            .ToListAsync();
    }
}