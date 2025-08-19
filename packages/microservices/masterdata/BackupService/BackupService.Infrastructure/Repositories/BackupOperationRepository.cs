using BackupService.Core.Entities;
using BackupService.Core.Interfaces;
using BackupService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackupService.Infrastructure.Repositories;

public class BackupOperationRepository : IBackupOperationRepository
{
    private readonly BackupServiceDbContext _dbContext;
    private readonly ILogger<BackupOperationRepository> _logger;

    public BackupOperationRepository(
        BackupServiceDbContext dbContext,
        ILogger<BackupOperationRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task AddAsync(BackupOperationRecord operationRecord)
    {
        if (!Guid.TryParse(operationRecord.CommandId, out _))
        {
            throw new ArgumentException("Invalid CommandId format", nameof(operationRecord.CommandId));
        }

        var operation = new BackupOperationRecord
        {
            CommandId = operationRecord.CommandId,
            CommandType = operationRecord.CommandType,
            InitiatedAt = operationRecord.InitiatedAt,
            CompletedAt = operationRecord.CompletedAt,
            IsSuccessful = operationRecord.IsSuccessful,
            Status = operationRecord.Status,
            ResultMessage = operationRecord.ResultMessage,
            IsCritical = operationRecord.IsCritical,
            ChainId = operationRecord.ChainId,
            ServiceResponses = operationRecord.ServiceResponses?.Select(r => new BackupServiceResponseRecord
            {
                Id = Guid.NewGuid(),
                CommandId = r.CommandId,
                ServiceName = r.ServiceName,
                IsSuccessful = r.IsSuccessful,
                Message = r.Message,
                ErrorDetails = r.ErrorDetails,
                ProcessedAt = r.ProcessedAt
            }).ToList() ?? new List<BackupServiceResponseRecord>()
        };

        _dbContext.BackupOperations.Add(operation);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<BackupOperationRecord?> GetByCommandIdAsync(string commandId)
    {
        return await _dbContext.BackupOperations
            .Include(o => o.ServiceResponses)
            .FirstOrDefaultAsync(o => o.CommandId == commandId);
    }

    public async Task<List<BackupOperationRecord>> GetRecentAsync(int count)
    {
        return await _dbContext.BackupOperations
            .Include(o => o.ServiceResponses)
            .OrderByDescending(o => o.InitiatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task UpdateAsync(BackupOperationRecord operationRecord)
    {
        var existingOperation = await _dbContext.BackupOperations
            .Include(o => o.ServiceResponses)
            .FirstOrDefaultAsync(o => o.CommandId == operationRecord.CommandId);

        if (existingOperation == null)
        {
            throw new InvalidOperationException($"No backup operation found for command {operationRecord.CommandId}");
        }

        existingOperation.CommandType = operationRecord.CommandType;
        existingOperation.InitiatedAt = operationRecord.InitiatedAt;
        existingOperation.CompletedAt = operationRecord.CompletedAt;
        existingOperation.IsSuccessful = operationRecord.IsSuccessful;
        existingOperation.Status = operationRecord.Status;
        existingOperation.ResultMessage = operationRecord.ResultMessage;
        existingOperation.IsCritical = operationRecord.IsCritical;
        existingOperation.ChainId = operationRecord.ChainId;

        if (existingOperation.ServiceResponses != null && existingOperation.ServiceResponses.Any())
        {
            _dbContext.ServiceResponses.RemoveRange(existingOperation.ServiceResponses);
        }  
        existingOperation.ServiceResponses = operationRecord.ServiceResponses?.Select(r => new BackupServiceResponseRecord
        {
            Id = Guid.NewGuid(),
            CommandId = r.CommandId,
            ServiceName = r.ServiceName,
            IsSuccessful = r.IsSuccessful,
            Message = r.Message,
            ErrorDetails = r.ErrorDetails,
            ProcessedAt = r.ProcessedAt
        }).ToList() ?? new List<BackupServiceResponseRecord>();

        await _dbContext.SaveChangesAsync();
    }
}