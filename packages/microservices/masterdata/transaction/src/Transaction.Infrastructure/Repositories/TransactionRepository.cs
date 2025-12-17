using Microsoft.EntityFrameworkCore;
using Transaction.Core.DTOs;
using Transaction.Core.Entities;
using Transaction.Core.Interfaces;
using Transaction.Core.Services;
using Transaction.Infrastructure.Data;

namespace Transaction.Infrastructure.Repositories;

public class TransactionRepository : Repository<WeighbridgeTransaction>, ITransactionRepository
{
    public TransactionRepository(TransactionDbContext context, ITimeService timeService) : base(context, timeService)
    {
    }

    public async Task<bool> IsReceiptNoAvailableAsync(string receiptNo)
    {
        return !await _dbSet.AnyAsync(e => e.ReceiptNo.ToLower() == receiptNo.ToLower() && !e.IsDeleted);
    }

    public async Task<WeighbridgeTransaction?> GetByReceiptNoAsync(string receiptNo)
    {
        return await _dbSet
            .FirstOrDefaultAsync(e => e.ReceiptNo.ToLower() == receiptNo.ToLower() && !e.IsDeleted);
    }

    public async Task<PagedResult<WeighbridgeTransaction>> GetPagedAsync(WeighbridgeTransactionFilter filter)
    {
        var query = _dbSet.Where(t => !t.IsDeleted).AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.ReceiptNo))
        {
            query = query.Where(t => t.ReceiptNo.Contains(filter.ReceiptNo));
        }

        if (!string.IsNullOrWhiteSpace(filter.NoPlate))
        {
            query = query.Where(t => t.NoPlate.Contains(filter.NoPlate));
        }

        if (!string.IsNullOrWhiteSpace(filter.DriverName))
        {
            query = query.Where(t => t.DriverName.Contains(filter.DriverName));
        }

        if (filter.VehicleId.HasValue)
        {
            query = query.Where(t => t.VehicleId == filter.VehicleId.Value);
        }

        if (filter.ProductId.HasValue)
        {
            query = query.Where(t => t.CommodityId == filter.ProductId.Value);
        }

        if (filter.SupplierId.HasValue)
        {
            query = query.Where(t => t.SupplierId == filter.SupplierId.Value);
        }

        if (filter.CustomerId.HasValue)
        {
            query = query.Where(t => t.CustomerId == filter.CustomerId.Value);
        }

        if (filter.TransporterId.HasValue)
        {
            query = query.Where(t => t.TransporterId == filter.TransporterId.Value);
        }

        if (filter.OriginId.HasValue)
        {
            query = query.Where(t => t.OriginId == filter.OriginId.Value);
        }

        if (filter.DestinationId.HasValue)
        {
            query = query.Where(t => t.DestinationId == filter.DestinationId.Value);
        }

        if (filter.WeighBridgeId.HasValue)
        {
            query = query.Where(t => t.WeighBridgeId == filter.WeighBridgeId.Value);
        }

        if (filter.OperatorId.HasValue)
        {
            query = query.Where(t => t.OperatorId == filter.OperatorId.Value || t.OperatorId2nd == filter.OperatorId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(t => t.Status == filter.Status.Value);
        }

        if (filter.IsCompleted.HasValue)
        {
            query = query.Where(t => t.IsCompleted == filter.IsCompleted.Value);
        }

        if (filter.StartDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= filter.EndDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.WeighMode))
            if (!string.IsNullOrWhiteSpace(filter.WeighMode))
            {
                if (Enum.TryParse<WeighingDirection>(filter.WeighMode, true, out var weighMode))
                {
                    query = query.Where(t => t.WeighMode == weighMode);
                }
            }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = filter.SortBy.ToLower() switch
        {
            "receiptno" => filter.SortDescending ? query.OrderByDescending(t => t.ReceiptNo) : query.OrderBy(t => t.ReceiptNo),
            "noplate" => filter.SortDescending ? query.OrderByDescending(t => t.NoPlate) : query.OrderBy(t => t.NoPlate),
            "drivername" => filter.SortDescending ? query.OrderByDescending(t => t.DriverName) : query.OrderBy(t => t.DriverName),
            "status" => filter.SortDescending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
            "completeddate" => filter.SortDescending ? query.OrderByDescending(t => t.CompletedDate) : query.OrderBy(t => t.CompletedDate),
            _ => filter.SortDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt)
        };

        // Apply pagination
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<WeighbridgeTransaction>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public async Task<List<WeighbridgeTransaction>> GetIncompleteTransactionsByVehicleAsync(string noPlate)
    {
        return await _dbSet
            .Where(t => t.NoPlate == noPlate && !t.IsCompleted && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<WeighbridgeTransaction>> GetIncompleteTransactionsByVehicleIdAsync(string vehicleId)
    {
        // Try to parse as Guid
        if (Guid.TryParse(vehicleId, out Guid vehicleIdGuid))
        {
            return await _dbSet
                .Where(t => t.VehicleId == vehicleIdGuid && !t.IsCompleted && !t.IsDeleted)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
        
        // If not a valid Guid, return empty list or throw an exception
        // depending on your requirements
        return new List<WeighbridgeTransaction>();
    }

    public async Task<List<WeighbridgeTransaction>> GetTransactionsByStatusAsync(WeighbridgeTransactionStatus status, int limit = 100)
    {
        return await _dbSet
            .Where(t => t.Status == status && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<WeighbridgeTransaction?> GetWithWeighingRecordsAsync(string id)
    {
        var transaction = await _dbSet
            .Include(t => t.WeighingRecords)
            .Include(t => t.ReweighRecords)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
            
        if (transaction?.WeighingRecords != null)
        {
            // Order the weighing records by sequence
            transaction.WeighingRecords = transaction.WeighingRecords
                .OrderBy(w => w.WeighingSequence)
                .ToList();
                
            // Ensure CompletedWeighings is in sync with the actual number of weighings
            if (transaction.WeighingRecords.Any())
            {
                var maxSequence = transaction.WeighingRecords.Max(w => w.WeighingSequence);
                if (maxSequence != transaction.CompletedWeighings)
                {
                    // Update the CompletedWeighings
                    transaction.CompletedWeighings = maxSequence;
                    transaction.UpdatedAt = DateTime.UtcNow;
                    
                    // Explicitly mark the entity as modified
                    _context.Entry(transaction).Property(x => x.CompletedWeighings).IsModified = true;
                    _context.Entry(transaction).Property(x => x.UpdatedAt).IsModified = true;
                    
                    // Save changes
                    await _context.SaveChangesAsync();
                    
                    // Detach the entity to avoid tracking issues
                    _context.Entry(transaction).State = EntityState.Detached;
                    
                    // Reload the transaction with includes
                    transaction = await _dbSet
                        .Include(t => t.WeighingRecords)
                        .Include(t => t.ReweighRecords)
                        .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
                }
            }
            else
            {
                // If no weighing records, ensure CompletedWeighings is 0
                if (transaction.CompletedWeighings != 0)
                {
                    transaction.CompletedWeighings = 0;
                    transaction.UpdatedAt = DateTime.UtcNow;
                    
                    _context.Entry(transaction).Property(x => x.CompletedWeighings).IsModified = true;
                    _context.Entry(transaction).Property(x => x.UpdatedAt).IsModified = true;
                    
                    await _context.SaveChangesAsync();
                    
                    _context.Entry(transaction).State = EntityState.Detached;
                    
                    transaction = await _dbSet
                        .Include(t => t.WeighingRecords)
                        .Include(t => t.ReweighRecords)
                        .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
                }
            }
        }
        
        return transaction;
    }

    public async Task<WeighbridgeTransaction?> GetWithAuditLogsAsync(string id)
    {
        return await _dbSet
            .Include(t => t.AuditLogs)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
    }
    
    public async Task<string?> GetLatestReceiptNumberAsync(string datePrefix)
    {
        return await _dbSet
            .Where(t => t.ReceiptNo.StartsWith(datePrefix) && !t.IsDeleted)
            .OrderByDescending(t => t.ReceiptNo)
            .Select(t => t.ReceiptNo)
            .FirstOrDefaultAsync();
    }
    
    public void MarkAsModified(WeighbridgeTransaction entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
    }
    
    public override async Task<WeighbridgeTransaction?> UpdateAsync(WeighbridgeTransaction entity)
    {
        var existingEntity = await _dbSet.FindAsync(entity.Id);
        if (existingEntity == null)
        {
            return null;
        }

        // Update all properties from the incoming entity to the existing entity
        _context.Entry(existingEntity).CurrentValues.SetValues(entity);
        
        // Explicitly set the UpdatedAt timestamp
        existingEntity.UpdatedAt = DateTime.UtcNow;
        
        // Explicitly mark the CompletedWeighings as modified to ensure it's updated
        _context.Entry(existingEntity).Property(x => x.CompletedWeighings).IsModified = true;
        
        // Mark the entity as modified to ensure all changes are saved
        _context.Entry(existingEntity).State = EntityState.Modified;
        
        await _context.SaveChangesAsync();
        return existingEntity;
    }
}