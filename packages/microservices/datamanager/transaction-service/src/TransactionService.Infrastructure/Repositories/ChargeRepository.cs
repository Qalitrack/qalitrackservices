using Microsoft.EntityFrameworkCore;
using TransactionService.Core.Entities;
using TransactionService.Core.Interfaces;
using TransactionService.Infrastructure.Data;

namespace TransactionService.Infrastructure.Repositories;

public class ChargeRepository : Repository<TransactionCharge>, IChargeRepository
{
    public ChargeRepository(TransactionDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TransactionCharge>> GetByTransactionIdAsync(string transactionId)
    {
        return await _dbSet
            .Where(c => c.TransactionId == transactionId && !c.IsDeleted)
            .OrderBy(c => c.ChargeType)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransactionCharge>> GetUnpaidChargesAsync(string transactionId)
    {
        return await _dbSet
            .Where(c => c.TransactionId == transactionId && !c.IsPaid && c.IsApproved && !c.IsDeleted)
            .OrderBy(c => c.ChargeType)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransactionCharge>> GetUnapprovedChargesAsync()
    {
        return await _dbSet
            .Include(c => c.Transaction)
            .Where(c => !c.IsApproved && !c.IsDeleted)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalAmountAsync(string transactionId)
    {
        return await _dbSet
            .Where(c => c.TransactionId == transactionId && c.IsApproved && !c.IsDeleted)
            .SumAsync(c => c.TotalAmount);
    }

    public async Task<decimal> GetUnpaidAmountAsync(string transactionId)
    {
        return await _dbSet
            .Where(c => c.TransactionId == transactionId && !c.IsPaid && c.IsApproved && !c.IsDeleted)
            .SumAsync(c => c.TotalAmount);
    }
}