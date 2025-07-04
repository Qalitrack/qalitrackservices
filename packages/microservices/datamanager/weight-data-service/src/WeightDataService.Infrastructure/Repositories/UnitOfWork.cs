using Microsoft.EntityFrameworkCore.Storage;
using WeightDataService.Core.Interfaces;
using WeightDataService.Infrastructure.Data;

namespace WeightDataService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly WeightDataContext _context;
    private IDbContextTransaction? _transaction;
    
    private IWeightMeasurementRepository? _weightMeasurements;
    private IWeighbridgeStatusRepository? _weighbridgeStatuses;
    private IWeightCorrectionRepository? _weightCorrections;

    public UnitOfWork(WeightDataContext context)
    {
        _context = context;
    }

    public IWeightMeasurementRepository WeightMeasurements =>
        _weightMeasurements ??= new WeightMeasurementRepository(_context);

    public IWeighbridgeStatusRepository WeighbridgeStatuses =>
        _weighbridgeStatuses ??= new WeighbridgeStatusRepository(_context);

    public IWeightCorrectionRepository WeightCorrections =>
        _weightCorrections ??= new WeightCorrectionRepository(_context);

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