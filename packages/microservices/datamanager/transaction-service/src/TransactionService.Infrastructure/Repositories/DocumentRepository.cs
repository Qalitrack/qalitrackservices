using Microsoft.EntityFrameworkCore;
using TransactionService.Core.Entities;
using TransactionService.Core.Interfaces;
using TransactionService.Infrastructure.Data;

namespace TransactionService.Infrastructure.Repositories;

public class DocumentRepository : Repository<TransactionDocument>, IDocumentRepository
{
    public DocumentRepository(TransactionDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TransactionDocument>> GetByTransactionIdAsync(string transactionId)
    {
        return await _dbSet
            .Where(d => d.TransactionId == transactionId && !d.IsDeleted)
            .OrderByDescending(d => d.Version)
            .ThenByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransactionDocument>> GetActiveByTransactionIdAsync(string transactionId)
    {
        return await _dbSet
            .Where(d => d.TransactionId == transactionId && d.IsActive && !d.IsDeleted)
            .OrderByDescending(d => d.Version)
            .ThenByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<TransactionDocument?> GetLatestVersionAsync(string transactionId, string documentType)
    {
        return await _dbSet
            .Where(d => d.TransactionId == transactionId && 
                       d.DocumentType == documentType && 
                       d.IsActive && 
                       !d.IsDeleted)
            .OrderByDescending(d => d.Version)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TransactionDocument>> GetByDocumentTypeAsync(string documentType)
    {
        return await _dbSet
            .Include(d => d.Transaction)
            .Where(d => d.DocumentType == documentType && d.IsActive && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> DocumentExistsAsync(string transactionId, string fileName)
    {
        return await _dbSet
            .AnyAsync(d => d.TransactionId == transactionId && 
                          d.FileName == fileName && 
                          d.IsActive && 
                          !d.IsDeleted);
    }
}