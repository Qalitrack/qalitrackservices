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
        // Read-only — never mutated by any caller of this method — so no
        // change tracking needed.
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ReceiptNo.ToLower() == receiptNo.ToLower() && !e.IsDeleted);
    }

    public async Task<WeighbridgeTransaction?> GetByIdAsync(string ticketId)
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

        if (!string.IsNullOrWhiteSpace(filter.VehicleID))
        {
            query = query.Where(t => t.VehicleID == filter.VehicleID);
        }

        if (!string.IsNullOrWhiteSpace(filter.CommodityID))
        {
            query = query.Where(t => t.CommodityID == filter.CommodityID);
        }

        if (!string.IsNullOrWhiteSpace(filter.SupplierID))
        {
            query = query.Where(t => t.SupplierID == filter.SupplierID);
        }

        if (!string.IsNullOrWhiteSpace(filter.CustomerID))
        {
            query = query.Where(t => t.CustomerID == filter.CustomerID);
        }

        if (!string.IsNullOrWhiteSpace(filter.TransporterID))
        {
            query = query.Where(t => t.TransporterID == filter.TransporterID);
        }

        if (!string.IsNullOrWhiteSpace(filter.OriginID))
        {
            query = query.Where(t => t.OriginID == filter.OriginID);
        }

        if (!string.IsNullOrWhiteSpace(filter.DestinationID))
        {
            query = query.Where(t => t.DestinationID == filter.DestinationID);
        }

        if (!string.IsNullOrWhiteSpace(filter.WeighBridgeID))
        {
            query = query.Where(t => t.WeighBridgeID == filter.WeighBridgeID);
        }

        if (!string.IsNullOrWhiteSpace(filter.OperatorID))
        {
            query = query.Where(t => t.OperatorID == filter.OperatorID || 
                                     t.OperatorID2nd == filter.OperatorID);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(t => t.Status == filter.Status);
        }

        if (filter.IsCompleted.HasValue)
        {
            query = filter.IsCompleted.Value
                ? query.Where(t => t.Status == "Completed")
                : query.Where(t => t.Status != "Completed");
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

        // Apply pagination — read-only listing, never mutated downstream.
        var items = await query
            .AsNoTracking()
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

    private static readonly TimeSpan StuckThreshold = TimeSpan.FromHours(2);

    public async Task<TransactionStatsDto> GetStatsAsync()
    {
        var now = DateTime.Now;
        var today = now.Date;
        var tomorrow = today.AddDays(1);
        var weekStart = today.AddDays(-6);
        var stuckCutoff = now - StuckThreshold;

        var baseQuery = _dbSet.Where(t => !t.IsDeleted);

        // Counts, groupings, and the "oldest active" lookup are all pushed down
        // to SQL as separate small aggregate queries instead of pulling every
        // row into memory and counting/grouping in C#.
        var totalCount = await baseQuery.CountAsync();
        var completedCount = await baseQuery.CountAsync(t => t.Status == "Completed");
        var activeCount = await baseQuery.CountAsync(t => t.Status == "Active");
        var todayCount = await baseQuery.CountAsync(t => t.FirstWeightDate >= today && t.FirstWeightDate < tomorrow);
        var todayCompletedCount = await baseQuery.CountAsync(t => t.Status == "Completed" && t.FirstWeightDate >= today && t.FirstWeightDate < tomorrow);
        var thisWeekCount = await baseQuery.CountAsync(t => t.FirstWeightDate >= weekStart);

        // Aging: how many still-Active tickets have been waiting past a
        // reasonable turnaround threshold, and how old the longest-waiting one
        // is — a raw "Pending W2" count doesn't tell an operator whether those
        // tickets are 5 minutes old (normal) or 5 hours old (something's stuck).
        var stuckCount = await baseQuery.CountAsync(t => t.Status == "Active" && t.FirstWeightDate <= stuckCutoff);
        var oldestActiveFirstWeightDate = await baseQuery
            .Where(t => t.Status == "Active")
            .OrderBy(t => t.FirstWeightDate)
            .Select(t => (DateTime?)t.FirstWeightDate)
            .FirstOrDefaultAsync();
        var oldestActiveAgeMinutes = oldestActiveFirstWeightDate.HasValue
            ? (int)(now - oldestActiveFirstWeightDate.Value).TotalMinutes
            : (int?)null;

        var trendCounts = await baseQuery
            .Where(t => t.FirstWeightDate >= weekStart)
            .GroupBy(t => t.FirstWeightDate.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();
        var trendLookup = trendCounts.ToDictionary(x => x.Date, x => x.Count);
        var weeklyTrend = Enumerable.Range(0, 7)
            .Select(i => weekStart.AddDays(i))
            .Select(d => new DailyCountDto { Date = d, Count = trendLookup.TryGetValue(d, out var c) ? c : 0 })
            .ToList();

        var topVehicles = await baseQuery
            .Where(t => !string.IsNullOrWhiteSpace(t.NoPlate))
            .GroupBy(t => t.NoPlate)
            .Select(g => new NameCountDto { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        var commodityMix = await baseQuery
            .Where(t => !string.IsNullOrWhiteSpace(t.CommodityName))
            .GroupBy(t => t.CommodityName)
            .Select(g => new NameCountDto { Name = g.Key!, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        static decimal ParseWeight(string? w) => decimal.TryParse(w, out var v) ? v : 0m;

        // NetWeight is stored as text, so summing it can't be pushed to SQL
        // without a schema change (migrating it to numeric) — this is the one
        // place still touching every row, but now only for this single column
        // instead of the full 5-column projection the old version pulled.
        var netWeights = await baseQuery
            .Select(t => new { t.NetWeight, t.FirstWeightDate })
            .ToListAsync();
        var totalNetWeight = netWeights.Sum(r => ParseWeight(r.NetWeight));
        var todayNetWeight = netWeights
            .Where(r => r.FirstWeightDate >= today && r.FirstWeightDate < tomorrow)
            .Sum(r => ParseWeight(r.NetWeight));

        return new TransactionStatsDto
        {
            TotalCount = totalCount,
            CompletedCount = completedCount,
            ActiveCount = activeCount,
            OtherCount = totalCount - completedCount - activeCount,
            TodayCount = todayCount,
            TodayCompletedCount = todayCompletedCount,
            ThisWeekCount = thisWeekCount,
            TotalNetWeight = totalNetWeight,
            TodayNetWeight = todayNetWeight,
            WeeklyTrend = weeklyTrend,
            TopVehicles = topVehicles,
            CommodityMix = commodityMix,
            StuckCount = stuckCount,
            OldestActiveAgeMinutes = oldestActiveAgeMinutes
        };
    }

    public async Task<List<WeighbridgeTransaction>> GetIncompleteTransactionsByVehicleAsync(string noPlate)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(t => t.NoPlate == noPlate &&
                       (t.Status == "Active" || t.Status == "InProgress") &&
                       !t.IsDeleted)
            .OrderByDescending(t => t.FirstWeightDate)
            .ToListAsync();
    }

    public async Task<List<WeighbridgeTransaction>> GetIncompleteTransactionsByVehicleIdAsync(string vehicleId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(t => t.VehicleID == vehicleId &&
                       (t.Status == "Active" || t.Status == "InProgress") &&
                       !t.IsDeleted)
            .OrderByDescending(t => t.FirstWeightDate)
            .ToListAsync();
    }

    public async Task<List<WeighbridgeTransaction>> GetTransactionsByStatusAsync(string status, int limit = 100)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(t => t.Status == status && !t.IsDeleted)
            .OrderByDescending(t => t.FirstWeightDate)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<ReweighRecord>> GetReweighRecordsAsync(string ticketId)
    {
        return await _context.ReweighRecords
            .AsNoTracking()
            .Where(r => r.WeighbridgeTransactionId == ticketId && !r.IsDeleted)
            .OrderBy(r => r.AttemptNumber)
            .ToListAsync();
    }

    public async Task<ReweighRecord> CreateReweighRecordAsync(ReweighRecord record)
    {
        record.CreatedAt = DateTime.UtcNow;
        record.UpdatedAt = DateTime.UtcNow;

        await _context.ReweighRecords.AddAsync(record);
        await _context.SaveChangesAsync();

        return record;
    }

    public async Task<string?> GetLatestReceiptNumberAsync(string datePrefix)
    {
        return await _dbSet
            .Where(t => t.ReceiptNo.StartsWith(datePrefix) && !t.IsDeleted)
            .OrderByDescending(t => t.ReceiptNo)
            .Select(t => t.ReceiptNo)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteAsync(string ticketId, string? changedBy = null)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(t => t.TicketID == ticketId);
        if (entity == null)
        {
            return false;
        }

        var oldValues = System.Text.Json.JsonSerializer.Serialize(entity);

        // Soft delete
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = changedBy;

        _context.TransactionAuditLogs.Add(new TransactionAuditLog
        {
            WeighbridgeTransactionId = ticketId,
            Action = "Deleted",
            ChangedBy = changedBy ?? string.Empty,
            ChangedFields = "[]",
            OldValues = oldValues,
            NewValues = string.Empty,
            ChangeTimestamp = DateTime.UtcNow,
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<TransactionAuditLog>> GetAuditLogsAsync(string ticketId)
    {
        return await _context.TransactionAuditLogs
            .AsNoTracking()
            .Where(a => a.WeighbridgeTransactionId == ticketId && !a.IsDeleted)
            .OrderByDescending(a => a.ChangeTimestamp)
            .ToListAsync();
    }

    public async Task<TransactionAuditLog> CreateAuditLogAsync(TransactionAuditLog log)
    {
        log.ChangeTimestamp = log.ChangeTimestamp == default ? DateTime.UtcNow : log.ChangeTimestamp;
        log.CreatedAt = DateTime.UtcNow;
        log.UpdatedAt = DateTime.UtcNow;

        await _context.TransactionAuditLogs.AddAsync(log);
        await _context.SaveChangesAsync();

        return log;
    }

    public async Task<WeighbridgeTransaction> CreateWithUniqueReceiptNoAsync(WeighbridgeTransaction entity, Func<Task<string>> generateReceiptNo)
    {
        const int maxAttempts = 5;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            entity.ReceiptNo = await generateReceiptNo();

            try
            {
                return await CreateAsync(entity);
            }
            catch (DbUpdateException ex) when (attempt < maxAttempts && IsReceiptNoConflict(ex))
            {
                // Another concurrent request generated the same receipt
                // number and committed first. Detach so the next attempt's
                // Add() re-tracks cleanly, then regenerate and retry.
                _context.Entry(entity).State = EntityState.Detached;
            }
        }

        throw new InvalidOperationException(
            $"Failed to generate a unique receipt number after {maxAttempts} attempts due to concurrent ticket creation.");
    }

    private static bool IsReceiptNoConflict(DbUpdateException ex)
    {
        return ex.InnerException is Npgsql.PostgresException { SqlState: "23505" } pgEx &&
               pgEx.ConstraintName != null &&
               pgEx.ConstraintName.Contains("ReceiptNo", StringComparison.OrdinalIgnoreCase);
    }

    public override async Task<WeighbridgeTransaction?> UpdateAsync(WeighbridgeTransaction entity)
    {
        // Every caller in this service fetches the entity via GetByIdAsync
        // (tracked) and mutates it in place before calling this — `entity` is
        // therefore already the tracked instance in this same DbContext, so
        // re-querying it here (the previous implementation) was a redundant
        // round trip that changed nothing: SetValues just copied the entity's
        // values back onto itself. EF's own change tracking already knows
        // what changed; we just need to stamp UpdatedAt and save.
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Entry(entity).State = EntityState.Modified;

        await _context.SaveChangesAsync();
        return entity;
    }
}