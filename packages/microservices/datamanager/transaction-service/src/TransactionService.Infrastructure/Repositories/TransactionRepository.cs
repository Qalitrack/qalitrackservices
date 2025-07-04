using Microsoft.EntityFrameworkCore;
using TransactionService.Core.Entities;
using TransactionService.Core.Interfaces;
using TransactionService.Infrastructure.Data;

namespace TransactionService.Infrastructure.Repositories;

public class TransactionRepository : Repository<WeighingTransaction>, ITransactionRepository
{
    public TransactionRepository(TransactionDbContext context) : base(context)
    {
    }

    public async Task<WeighingTransaction?> GetWithWorkflowAsync(string id)
    {
        return await _dbSet
            .Include(t => t.WorkflowSteps.OrderBy(w => w.Order))
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
    }

    public async Task<WeighingTransaction?> GetWithChargesAsync(string id)
    {
        return await _dbSet
            .Include(t => t.Charges)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
    }

    public async Task<WeighingTransaction?> GetWithDocumentsAsync(string id)
    {
        return await _dbSet
            .Include(t => t.Documents.Where(d => d.IsActive))
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
    }

    public async Task<WeighingTransaction?> GetCompleteAsync(string id)
    {
        return await _dbSet
            .Include(t => t.WorkflowSteps.OrderBy(w => w.Order))
            .Include(t => t.Charges)
            .Include(t => t.Documents.Where(d => d.IsActive))
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
    }

    public async Task<IEnumerable<WeighingTransaction>> GetByVehicleIdAsync(string vehicleId)
    {
        return await _dbSet
            .Where(t => t.VehicleId == vehicleId && !t.IsDeleted)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeighingTransaction>> GetByOrganizationIdAsync(string organizationId)
    {
        return await _dbSet
            .Where(t => t.OrganizationId == organizationId && !t.IsDeleted)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeighingTransaction>> GetByStatusAsync(TransactionStatus status)
    {
        return await _dbSet
            .Where(t => t.Status == status && !t.IsDeleted)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeighingTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate && !t.IsDeleted)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<string> GenerateTransactionNumberAsync()
    {
        var today = DateTime.Today;
        var prefix = $"TXN{today:yyyyMMdd}";
        
        var lastTransaction = await _dbSet
            .Where(t => t.TransactionNumber.StartsWith(prefix))
            .OrderByDescending(t => t.TransactionNumber)
            .FirstOrDefaultAsync();

        if (lastTransaction == null)
        {
            return $"{prefix}001";
        }

        var lastNumber = lastTransaction.TransactionNumber.Substring(prefix.Length);
        if (int.TryParse(lastNumber, out var number))
        {
            return $"{prefix}{(number + 1):D3}";
        }

        return $"{prefix}001";
    }
}