using Microsoft.EntityFrameworkCore.Storage;
using DataSyncService.Core.Interfaces;
using DataSyncService.Infrastructure.Data;

namespace DataSyncService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly DataSyncDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(DataSyncDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}