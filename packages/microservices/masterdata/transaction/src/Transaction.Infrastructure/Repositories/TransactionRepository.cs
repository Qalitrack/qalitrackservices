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

    public async Task<WeighbridgeTransaction?> GetByIdAsync(int ticketId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => t.TicketID == ticketId && !t.IsDeleted);
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

        if (filter.VehicleID.HasValue)
        {
            query = query.Where(t => t.VehicleID == filter.VehicleID.Value);
        }

        if (filter.CommodityID.HasValue)
        {
            query = query.Where(t => t.CommodityID == filter.CommodityID.Value);
        }

        if (filter.SupplierID.HasValue)
        {
            query = query.Where(t => t.SupplierID == filter.SupplierID.Value);
        }

        if (filter.CustomerID.HasValue)
        {
            query = query.Where(t => t.CustomerID == filter.CustomerID.Value);
        }

        if (filter.TransporterID.HasValue)
        {
            query = query.Where(t => t.TransporterID == filter.TransporterID.Value);
        }

        if (filter.OriginID.HasValue)
        {
            query = query.Where(t => t.OriginID == filter.OriginID.Value);
        }

        if (filter.DestinationID.HasValue)
        {
            query = query.Where(t => t.DestinationID == filter.DestinationID.Value);
        }

        if (filter.WeighBridgeID.HasValue)
        {
            query = query.Where(t => t.WeighBridgeID == filter.WeighBridgeID.Value);
        }

        if (filter.OperatorID.HasValue)
        {
            query = query.Where(t => t.OperatorID == filter.OperatorID.Value || 
                                     (t.OperatorID2nd != null && t.OperatorID2nd == filter.OperatorID.Value.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(t => t.Status == filter.Status);
        }

        if (filter.StartDate.HasValue)
        {
            query = query.Where(t => t.FirstWeightDate >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            query = query.Where(t => t.FirstWeightDate <= filter.EndDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.WeighMode))
        {
            query = query.Where(t => t.WeighMode == filter.WeighMode);
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
            "secondweightdate" => filter.SortDescending ? query.OrderByDescending(t => t.SecondWeightDate) : query.OrderBy(t => t.SecondWeightDate),
            "firstweightdate" => filter.SortDescending ? query.OrderByDescending(t => t.FirstWeightDate) : query.OrderBy(t => t.FirstWeightDate),
            _ => filter.SortDescending ? query.OrderByDescending(t => t.FirstWeightDate) : query.OrderBy(t => t.FirstWeightDate)
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
            .Where(t => t.NoPlate == noPlate && 
                       (t.Status == "Active" || t.Status == "InProgress") && 
                       !t.IsDeleted)
            .OrderByDescending(t => t.FirstWeightDate)
            .ToListAsync();
    }

    public async Task<List<WeighbridgeTransaction>> GetIncompleteTransactionsByVehicleIdAsync(int vehicleId)
    {
        return await _dbSet
            .Where(t => t.VehicleID == vehicleId && 
                       (t.Status == "Active" || t.Status == "InProgress") && 
                       !t.IsDeleted)
            .OrderByDescending(t => t.FirstWeightDate)
            .ToListAsync();
    }

    public async Task<List<WeighbridgeTransaction>> GetTransactionsByStatusAsync(string status, int limit = 100)
    {
        return await _dbSet
            .Where(t => t.Status == status && !t.IsDeleted)
            .OrderByDescending(t => t.FirstWeightDate)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<ReweighRecord>> GetReweighRecordsAsync(int ticketId)
    {
        return await _context.ReweighRecords
            .Where(r => r.WeighbridgeTransactionId == ticketId && !r.IsDeleted)
            .OrderBy(r => r.AttemptNumber)
            .ToListAsync();
    }

    public async Task<string?> GetLatestReceiptNumberAsync(string datePrefix)
    {
        return await _dbSet
            .Where(t => t.ReceiptNo.StartsWith(datePrefix) && !t.IsDeleted)
            .OrderByDescending(t => t.ReceiptNo)
            .Select(t => t.ReceiptNo)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteAsync(int ticketId)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(t => t.TicketID == ticketId);
        if (entity == null)
        {
            return false;
        }

        // Soft delete
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public override async Task<WeighbridgeTransaction?> UpdateAsync(WeighbridgeTransaction entity)
    {
        var existingEntity = await _dbSet.FirstOrDefaultAsync(t => t.TicketID == entity.TicketID);
        if (existingEntity == null)
        {
            return null;
        }

        // Update all properties from the incoming entity to the existing entity
        _context.Entry(existingEntity).CurrentValues.SetValues(entity);
        
        // Explicitly set the UpdatedAt timestamp
        existingEntity.UpdatedAt = DateTime.UtcNow;
        
        // Mark the entity as modified to ensure all changes are saved
        _context.Entry(existingEntity).State = EntityState.Modified;
        
        await _context.SaveChangesAsync();
        return existingEntity;
    }
}
