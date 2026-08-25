using System.Linq;
using System.Text.Json;
using AutoMapper;
using Transaction.Core.DTOs;
using Transaction.Core.Entities;
using Transaction.Core.Interfaces;

namespace Transaction.Core.Services;

public class TransactionService(
    ITransactionRepository transactionRepository,
    IMapper mapper,
    ITimeService timeService,
    IReceiptNumberService receiptNumberService)
    : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ITimeService _timeService = timeService ?? throw new ArgumentNullException(nameof(timeService));
    private readonly IReceiptNumberService _receiptNumberService = receiptNumberService ?? throw new ArgumentNullException(nameof(receiptNumberService));

    // Bookkeeping fields excluded from the before/after diff — they change on
    // every mutation as a side effect and aren't "what actually changed."
    private static readonly string[] AuditIgnoredProperties =
    {
        nameof(BaseEntity.Id), nameof(BaseEntity.CreatedAt), nameof(BaseEntity.UpdatedAt),
        nameof(BaseEntity.CreatedBy), nameof(BaseEntity.UpdatedBy), nameof(BaseEntity.IsDeleted),
        nameof(WeighbridgeTransaction.TicketID)
    };

    // Snapshots a transaction's business-field values before mutation, since
    // the entity fetched via GetByIdAsync is EF-tracked and gets mutated in
    // place by the caller — there's no other way to observe the "before" state.
    private static Dictionary<string, object?> SnapshotValues(WeighbridgeTransaction entity)
    {
        var snapshot = new Dictionary<string, object?>();
        foreach (var prop in typeof(WeighbridgeTransaction).GetProperties())
        {
            if (AuditIgnoredProperties.Contains(prop.Name)) continue;
            snapshot[prop.Name] = prop.GetValue(entity);
        }
        return snapshot;
    }

    // Diffs `before` (a snapshot taken via SnapshotValues) against `after`'s
    // current values and, if anything actually changed, returns an audit row
    // ready to persist. Returns null when nothing changed (e.g. an Update
    // call whose DTO fields all matched the existing values).
    private static TransactionAuditLog? BuildAuditLog(
        WeighbridgeTransaction after, Dictionary<string, object?> before,
        string action, string? changedBy, string? reason)
    {
        var changedFields = new List<string>();
        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();

        foreach (var prop in typeof(WeighbridgeTransaction).GetProperties())
        {
            if (AuditIgnoredProperties.Contains(prop.Name)) continue;
            var oldVal = before.TryGetValue(prop.Name, out var v) ? v : null;
            var newVal = prop.GetValue(after);
            if (!Equals(oldVal, newVal))
            {
                changedFields.Add(prop.Name);
                oldValues[prop.Name] = oldVal;
                newValues[prop.Name] = newVal;
            }
        }

        if (changedFields.Count == 0 && action == "Updated")
        {
            return null;
        }

        return new TransactionAuditLog
        {
            WeighbridgeTransactionId = after.TicketID,
            Action = action,
            ChangedBy = changedBy ?? string.Empty,
            ChangedFields = JsonSerializer.Serialize(changedFields),
            OldValues = JsonSerializer.Serialize(oldValues),
            NewValues = JsonSerializer.Serialize(newValues),
            ChangeTimestamp = DateTime.UtcNow,
            Reason = reason ?? string.Empty,
        };
    }

    public async Task<PagedResult<TransactionReadDto>> GetAllAsync(WeighbridgeTransactionFilter filter)
    {
        var pagedTransactions = await _transactionRepository.GetPagedAsync(filter);
        
        var dtos = _mapper.Map<List<TransactionReadDto>>(pagedTransactions.Items);
        
        return new PagedResult<TransactionReadDto>
        {
            Items = dtos,
            TotalCount = pagedTransactions.TotalCount,
            PageNumber = pagedTransactions.PageNumber,
            PageSize = pagedTransactions.PageSize
        };
    }

    public async Task<TransactionStatsDto> GetStatsAsync()
    {
        return await _transactionRepository.GetStatsAsync();
    }

    public async Task<TransactionReadDto?> GetByIdAsync(string ticketId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(ticketId);
        return transaction == null ? null : _mapper.Map<TransactionReadDto>(transaction);
    }

    public async Task<TransactionReadDto?> GetByReceiptNoAsync(string receiptNo)
    {
        var transaction = await _transactionRepository.GetByReceiptNoAsync(receiptNo);
        return transaction == null ? null : _mapper.Map<TransactionReadDto>(transaction);
    }

    /// <summary>
    /// Creates a new weighbridge transaction with auto-generated TicketID and ReceiptNo.
    /// Receipt number format: QSL-YYYYMMDD-XXXXXX (e.g., QSL-20240202-000001)
    /// </summary>
    public async Task<TransactionReadDto> CreateAsync(CreateTransactionDto dto)
    {
        // Validate first weight
        if (string.IsNullOrWhiteSpace(dto.FirstWeight))
        {
            throw new ArgumentException("First weight is required.", nameof(dto.FirstWeight));
        }

        if (!decimal.TryParse(dto.FirstWeight, out var firstWeightValue))
        {
            throw new ArgumentException("First weight must be a valid number.", nameof(dto.FirstWeight));
        }

        if (firstWeightValue < 0)
        {
            throw new ArgumentException("First weight cannot be negative.", nameof(dto.FirstWeight));
        }

        var utcNow = _timeService.UtcNow;

        // Map DTO to entity
        var transaction = _mapper.Map<WeighbridgeTransaction>(dto);

        // Auto-generate GUID for TicketID if not already set
        if (string.IsNullOrEmpty(transaction.TicketID))
        {
            transaction.TicketID = Guid.NewGuid().ToString();
        }

        // Set timestamps
        transaction.CreatedAt = utcNow;
        transaction.UpdatedAt = utcNow;
        transaction.FirstWeightDate = utcNow;

        // Set initial status
        transaction.Status = "Active";

        // Receipt number in format: QSL-YYYYMMDD-XXXXXX (e.g. QSL-20240202-000001).
        // Generated and assigned inside CreateWithUniqueReceiptNoAsync, which
        // retries with a freshly-generated number if two concurrent creations
        // race to the same "next" receipt number (backed by a unique
        // constraint on ReceiptNo, so a collision fails loud instead of
        // silently duplicating).
        var createdTransaction = await _transactionRepository.CreateWithUniqueReceiptNoAsync(
            transaction, _receiptNumberService.GenerateReceiptNumberAsync);

        await _transactionRepository.CreateAuditLogAsync(new TransactionAuditLog
        {
            WeighbridgeTransactionId = createdTransaction.TicketID,
            Action = "Created",
            ChangedBy = string.Empty,
            ChangedFields = "[]",
            OldValues = string.Empty,
            NewValues = JsonSerializer.Serialize(SnapshotValues(createdTransaction)),
            ChangeTimestamp = utcNow,
        });

        // Return mapped DTO with the auto-generated ReceiptNo
        return _mapper.Map<TransactionReadDto>(createdTransaction);
    }

    public async Task<TransactionReadDto?> UpdateAsync(string ticketId, UpdateTransactionDto dto)
    {
        var existingTransaction = await _transactionRepository.GetByIdAsync(ticketId);
        if (existingTransaction == null)
        {
            return null;
        }

        // Prevent updating completed transactions
        if (existingTransaction.Status == "Completed")
        {
            throw new InvalidOperationException("Cannot update a completed transaction. Use reweigh workflow if changes are needed.");
        }

        // Prevent overwriting second weight if it already exists
        if (dto.SecondWeight != null && !string.IsNullOrEmpty(existingTransaction.SecondWeight))
        {
            throw new InvalidOperationException("Second weight has already been recorded. Use AddSecondWeight endpoint to record the second weight.");
        }

        var utcNow = _timeService.UtcNow;
        var before = SnapshotValues(existingTransaction);

        // Only update non-null fields from the DTO
        if (dto.NoPlate != null) existingTransaction.NoPlate = dto.NoPlate;
        if (dto.DriverName != null) existingTransaction.DriverName = dto.DriverName;
        if (dto.VehicleID != null) existingTransaction.VehicleID = dto.VehicleID;
        if (dto.CommodityID != null) existingTransaction.CommodityID = dto.CommodityID;
        if (dto.CommodityName != null) existingTransaction.CommodityName = dto.CommodityName;
        if (dto.SupplierID != null) existingTransaction.SupplierID = dto.SupplierID;
        if (dto.SupplierName != null) existingTransaction.SupplierName = dto.SupplierName;
        if (dto.CustomerID != null) existingTransaction.CustomerID = dto.CustomerID;
        if (dto.CustomerName != null) existingTransaction.CustomerName = dto.CustomerName;
        if (dto.TransporterID != null) existingTransaction.TransporterID = dto.TransporterID;
        if (dto.TransporterName != null) existingTransaction.TransporterName = dto.TransporterName;
        if (dto.OriginID != null) existingTransaction.OriginID = dto.OriginID;
        if (dto.OriginName != null) existingTransaction.OriginName = dto.OriginName;
        if (dto.DestinationID != null) existingTransaction.DestinationID = dto.DestinationID;
        if (dto.DestinationName != null) existingTransaction.DestinationName = dto.DestinationName;
        if (dto.WeighMode != null) existingTransaction.WeighMode = dto.WeighMode;
        if (dto.Operation != null) existingTransaction.Operation = dto.Operation;
        if (dto.SecondWeight != null) existingTransaction.SecondWeight = dto.SecondWeight;
        if (dto.WeighBridgeName2nd != null) existingTransaction.WeighBridgeName2nd = dto.WeighBridgeName2nd;
        if (dto.ScaleName2nd != null) existingTransaction.ScaleName2nd = dto.ScaleName2nd;
        if (dto.OperatorID2nd != null) existingTransaction.OperatorID2nd = dto.OperatorID2nd;
        if (dto.OperatorName2nd != null) existingTransaction.OperatorName2nd = dto.OperatorName2nd;
        if (dto.ChangeDesc != null) existingTransaction.ChangeDesc = dto.ChangeDesc;
        
        // Update timestamps
        existingTransaction.UpdatedAt = utcNow;
        existingTransaction.ChangeDate = utcNow;
        existingTransaction.UpdatedBy = dto.ChangedBy;

        // NOTE: ReceiptNo is NEVER updated - it's a permanent identifier

        var updatedTransaction = await _transactionRepository.UpdateAsync(existingTransaction);
        if (updatedTransaction != null)
        {
            var auditLog = BuildAuditLog(updatedTransaction, before, "Updated", dto.ChangedBy, dto.ChangeDesc);
            if (auditLog != null) await _transactionRepository.CreateAuditLogAsync(auditLog);
        }
        return updatedTransaction == null ? null : _mapper.Map<TransactionReadDto>(updatedTransaction);
    }

    public async Task<bool> DeleteAsync(string ticketId, string? changedBy = null)
    {
        var transaction = await _transactionRepository.GetByIdAsync(ticketId);
        if (transaction == null)
        {
            return false;
        }

        // Check if transaction is completed or has reweigh requested
        if (transaction.Status == "Completed" || transaction.Status == "ReweighRequested")
        {
            throw new InvalidOperationException("Cannot delete a completed transaction or a transaction with pending reweigh request.");
        }

        return await _transactionRepository.DeleteAsync(ticketId, changedBy);
    }

    public async Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(string ticketId)
    {
        var logs = await _transactionRepository.GetAuditLogsAsync(ticketId);
        return _mapper.Map<IEnumerable<AuditLogDto>>(logs);
    }

    public async Task<bool> IsReceiptNoAvailableAsync(string receiptNo)
    {
        return await _transactionRepository.IsReceiptNoAvailableAsync(receiptNo);
    }

    public async Task<TransactionReadDto?> AddSecondWeightAsync(AddSecondWeightDto dto)
    {
        var transaction = await _transactionRepository.GetByIdAsync(dto.TicketID);
        if (transaction == null)
        {
            return null;
        }

        // Check if transaction is already completed
        if (transaction.Status == "Completed")
        {
            throw new InvalidOperationException("Cannot add second weight to a completed transaction.");
        }

        // Check if second weight already exists
        if (!string.IsNullOrEmpty(transaction.SecondWeight))
        {
            throw new InvalidOperationException("Second weight has already been recorded for this transaction.");
        }

        // Validate second weight value
        if (string.IsNullOrWhiteSpace(dto.SecondWeight))
        {
            throw new ArgumentException("Second weight cannot be empty.", nameof(dto.SecondWeight));
        }

        if (!decimal.TryParse(dto.SecondWeight, out var secondWeightValue))
        {
            throw new ArgumentException("Second weight must be a valid number.", nameof(dto.SecondWeight));
        }

        if (secondWeightValue < 0)
        {
            throw new ArgumentException("Second weight cannot be negative.", nameof(dto.SecondWeight));
        }

        var utcNow = _timeService.UtcNow;
        var before = SnapshotValues(transaction);

        // Update second weighing information
        transaction.SecondWeight = dto.SecondWeight;
        transaction.WeighBridgeName2nd = dto.WeighBridgeName2nd;
        transaction.ScaleName2nd = dto.ScaleName2nd;
        transaction.OperatorID2nd = dto.OperatorID2nd;
        transaction.OperatorName2nd = dto.OperatorName2nd;
        transaction.SecondWeightDate = utcNow;
        transaction.UpdatedAt = utcNow;

        // Calculate net weight
        if (!decimal.TryParse(transaction.FirstWeight, out var firstWeight))
        {
            throw new InvalidOperationException("First weight is not a valid number.");
        }

        if (!decimal.TryParse(transaction.SecondWeight, out var secondWeight))
        {
            throw new InvalidOperationException("Second weight is not a valid number.");
        }

        var netWeight = Math.Abs(firstWeight - secondWeight);
        transaction.NetWeight = netWeight.ToString("F2");

        // Calculate turnaround time
        transaction.TurnaroundTime = transaction.SecondWeightDate.Value - transaction.FirstWeightDate;

        // Update status to completed if both weights are present
        if (!string.IsNullOrEmpty(transaction.FirstWeight) && !string.IsNullOrEmpty(transaction.SecondWeight))
        {
            transaction.Status = "Completed";
        }

        transaction.UpdatedBy = dto.ChangedBy;

        var updatedTransaction = await _transactionRepository.UpdateAsync(transaction);
        if (updatedTransaction != null)
        {
            var auditLog = BuildAuditLog(updatedTransaction, before, "SecondWeightAdded", dto.ChangedBy, dto.Notes);
            if (auditLog != null) await _transactionRepository.CreateAuditLogAsync(auditLog);
        }
        return updatedTransaction == null ? null : _mapper.Map<TransactionReadDto>(updatedTransaction);
    }

    public async Task<TransactionReadDto?> CompleteTransactionAsync(CompleteTransactionDto dto)
    {
        var transaction = await _transactionRepository.GetByIdAsync(dto.TicketID);
        if (transaction == null)
        {
            throw new ArgumentException("Transaction not found", nameof(dto.TicketID));
        }

        // Check if transaction is already completed
        if (transaction.Status == "Completed")
        {
            throw new InvalidOperationException("Transaction is already completed.");
        }

        // Verify both weights are present
        if (string.IsNullOrEmpty(transaction.FirstWeight) || string.IsNullOrEmpty(transaction.SecondWeight))
        {
            throw new InvalidOperationException("Cannot complete transaction. Both first and second weights are required.");
        }

        var utcNow = _timeService.UtcNow;
        var before = SnapshotValues(transaction);

        // Calculate net weight if not already calculated
        if (string.IsNullOrEmpty(transaction.NetWeight))
        {
            if (!decimal.TryParse(transaction.FirstWeight, out var firstWeight))
            {
                throw new InvalidOperationException("First weight is not a valid number.");
            }

            if (!decimal.TryParse(transaction.SecondWeight, out var secondWeight))
            {
                throw new InvalidOperationException("Second weight is not a valid number.");
            }

            var netWeight = Math.Abs(firstWeight - secondWeight);
            transaction.NetWeight = netWeight.ToString("F2");
        }

        // Calculate turnaround time if not already calculated
        if (transaction.TurnaroundTime == null)
        {
            if (transaction.SecondWeightDate.HasValue)
            {
                transaction.TurnaroundTime = transaction.SecondWeightDate.Value - transaction.FirstWeightDate;
            }
            else
            {
                throw new InvalidOperationException("Cannot calculate turnaround time: SecondWeightDate is not set.");
            }
        }

        transaction.Status = "Completed";
        transaction.UpdatedAt = utcNow;
        transaction.UpdatedBy = dto.ChangedBy;

        var updatedTransaction = await _transactionRepository.UpdateAsync(transaction);
        if (updatedTransaction != null)
        {
            var auditLog = BuildAuditLog(updatedTransaction, before, "Completed", dto.ChangedBy, null);
            if (auditLog != null) await _transactionRepository.CreateAuditLogAsync(auditLog);
        }
        return updatedTransaction == null ? null : _mapper.Map<TransactionReadDto>(updatedTransaction);
    }

    public async Task<IEnumerable<TransactionReadDto>> GetIncompleteTransactionsByVehicleAsync(string noPlate)
    {
        var transactions = await _transactionRepository.GetIncompleteTransactionsByVehicleAsync(noPlate);
        return _mapper.Map<IEnumerable<TransactionReadDto>>(transactions);
    }

    public async Task<IEnumerable<TransactionReadDto>> GetIncompleteTransactionsByVehicleIdAsync(string vehicleId)
    {
        var transactions = await _transactionRepository.GetIncompleteTransactionsByVehicleIdAsync(vehicleId);
        return _mapper.Map<IEnumerable<TransactionReadDto>>(transactions);
    }

    public async Task<IEnumerable<TransactionReadDto>> GetTransactionsByStatusAsync(string status, int limit = 100)
    {
        var transactions = await _transactionRepository.GetTransactionsByStatusAsync(status, limit);
        return _mapper.Map<IEnumerable<TransactionReadDto>>(transactions);
    }

    public async Task<bool> RequestReweighAsync(RequestReweighDto dto)
    {
        var transaction = await _transactionRepository.GetByIdAsync(dto.TicketID);
        if (transaction == null)
        {
            throw new ArgumentException("Transaction not found", nameof(dto.TicketID));
        }

        if (string.IsNullOrWhiteSpace(dto.Reason))
        {
            throw new ArgumentException("Reweigh reason is required.", nameof(dto.Reason));
        }

        if (transaction.Status == "ReweighRequested")
        {
            throw new InvalidOperationException("Reweigh has already been requested for this transaction.");
        }

        if (transaction.Status != "Completed")
        {
            throw new InvalidOperationException("Can only request reweigh for completed transactions.");
        }

        var utcNow = _timeService.UtcNow;

        // Get existing reweigh records to determine attempt number
        var existingRecords = await _transactionRepository.GetReweighRecordsAsync(dto.TicketID);
        var attemptNumber = existingRecords.Count + 1;

        // Create reweigh record for the request
        var reweighRecord = new ReweighRecord
        {
            WeighbridgeTransactionId = dto.TicketID,
            AttemptNumber = attemptNumber,
            StartedAt = utcNow,
            Status = "Pending",
            Reason = dto.Reason,
            Notes = "Reweigh requested, awaiting approval",
            Weight1 = decimal.TryParse(transaction.FirstWeight, out var fw) ? fw : null,
            Weight2 = decimal.TryParse(transaction.SecondWeight, out var sw) ? sw : null,
            NetWeight = decimal.TryParse(transaction.NetWeight, out var nw) ? nw : null,
            Weight1Timestamp = transaction.FirstWeightDate,
            Weight2Timestamp = transaction.SecondWeightDate,
            Operator1 = transaction.OperatorName,
            Operator2 = transaction.OperatorName2nd
        };

        await _transactionRepository.CreateReweighRecordAsync(reweighRecord);

        // Update reweigh permission
        transaction.ReweighPermission = dto.Reason;
        transaction.Status = "ReweighRequested";
        transaction.IsReweighed = true;
        transaction.UpdatedAt = utcNow;
        transaction.ChangeDate = utcNow;
        transaction.ChangeDesc = $"Reweigh requested: {dto.Reason}";

        await _transactionRepository.UpdateAsync(transaction);
        return true;
    }

    public async Task<IEnumerable<ReweighRecordDto>> GetReweighRecordsAsync(string ticketId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(ticketId);
        if (transaction == null)
        {
            throw new ArgumentException("Transaction not found", nameof(ticketId));
        }

        // Since ReweighRecords are stored separately, query them
        var reweighRecords = await _transactionRepository.GetReweighRecordsAsync(ticketId);
        return _mapper.Map<IEnumerable<ReweighRecordDto>>(reweighRecords ?? new List<ReweighRecord>());
    }

    public async Task<TransactionReadDto?> ApproveReweighAsync(ApproveReweighDto dto)
    {
        var transaction = await _transactionRepository.GetByIdAsync(dto.TicketID);
        if (transaction == null)
        {
            throw new ArgumentException("Transaction not found", nameof(dto.TicketID));
        }

        if (transaction.Status != "ReweighRequested")
        {
            throw new InvalidOperationException("Can only approve reweigh for transactions with ReweighRequested status.");
        }

        var utcNow = _timeService.UtcNow;

        // Get existing reweigh records to determine attempt number
        var existingRecords = await _transactionRepository.GetReweighRecordsAsync(dto.TicketID);
        var attemptNumber = existingRecords.Count + 1;

        // Create reweigh record for approval
        var reweighRecord = new ReweighRecord
        {
            WeighbridgeTransactionId = dto.TicketID,
            AttemptNumber = attemptNumber,
            StartedAt = utcNow,
            Status = "Approved",
            Reason = transaction.ReweighPermission,
            Notes = dto.Notes,
            PerformedBy = dto.ApprovedBy,
            Weight1 = decimal.TryParse(transaction.FirstWeight, out var fw) ? fw : null,
            Weight2 = decimal.TryParse(transaction.SecondWeight, out var sw) ? sw : null,
            NetWeight = decimal.TryParse(transaction.NetWeight, out var nw) ? nw : null,
            Weight1Timestamp = transaction.FirstWeightDate,
            Weight2Timestamp = transaction.SecondWeightDate,
            Operator1 = transaction.OperatorName,
            Operator2 = transaction.OperatorName2nd
        };

        await _transactionRepository.CreateReweighRecordAsync(reweighRecord);

        // Clear second weighing data to allow re-weighing
        transaction.SecondWeight = null;
        transaction.SecondWeightDate = null;
        transaction.NetWeight = null;
        transaction.TurnaroundTime = null;
        transaction.WeighBridgeName2nd = null;
        transaction.ScaleName2nd = null;
        transaction.OperatorID2nd = null;
        transaction.OperatorName2nd = null;

        // Reset status to Active
        transaction.Status = "Active";
        transaction.UpdatedAt = utcNow;
        transaction.ChangeDate = utcNow;
        transaction.ChangeDesc = $"Reweigh approved by {dto.ApprovedBy ?? "system"}. Previous weights cleared for re-weighing.";

        var updatedTransaction = await _transactionRepository.UpdateAsync(transaction);
        return updatedTransaction == null ? null : _mapper.Map<TransactionReadDto>(updatedTransaction);
    }

    public async Task<TransactionReadDto?> RejectReweighAsync(RejectReweighDto dto)
    {
        var transaction = await _transactionRepository.GetByIdAsync(dto.TicketID);
        if (transaction == null)
        {
            throw new ArgumentException("Transaction not found", nameof(dto.TicketID));
        }

        if (string.IsNullOrWhiteSpace(dto.RejectionReason))
        {
            throw new ArgumentException("Rejection reason is required.", nameof(dto.RejectionReason));
        }

        if (transaction.Status != "ReweighRequested")
        {
            throw new InvalidOperationException("Can only reject reweigh for transactions with ReweighRequested status.");
        }

        var utcNow = _timeService.UtcNow;

        // Get existing reweigh records to determine attempt number
        var existingRecords = await _transactionRepository.GetReweighRecordsAsync(dto.TicketID);
        var attemptNumber = existingRecords.Count + 1;

        // Create reweigh record for rejection
        var reweighRecord = new ReweighRecord
        {
            WeighbridgeTransactionId = dto.TicketID,
            AttemptNumber = attemptNumber,
            StartedAt = utcNow,
            CompletedAt = utcNow,
            Status = "Rejected",
            Reason = transaction.ReweighPermission,
            Notes = $"Rejection reason: {dto.RejectionReason}. {dto.Notes}",
            PerformedBy = dto.RejectedBy,
            Weight1 = decimal.TryParse(transaction.FirstWeight, out var fw) ? fw : null,
            Weight2 = decimal.TryParse(transaction.SecondWeight, out var sw) ? sw : null,
            NetWeight = decimal.TryParse(transaction.NetWeight, out var nw) ? nw : null,
            Weight1Timestamp = transaction.FirstWeightDate,
            Weight2Timestamp = transaction.SecondWeightDate,
            Operator1 = transaction.OperatorName,
            Operator2 = transaction.OperatorName2nd
        };

        await _transactionRepository.CreateReweighRecordAsync(reweighRecord);

        // Keep original weights and restore to Completed status
        transaction.Status = "Completed";
        transaction.UpdatedAt = utcNow;
        transaction.ChangeDate = utcNow;
        transaction.ChangeDesc = $"Reweigh rejected by {dto.RejectedBy ?? "system"}. Reason: {dto.RejectionReason}";

        var updatedTransaction = await _transactionRepository.UpdateAsync(transaction);
        return updatedTransaction == null ? null : _mapper.Map<TransactionReadDto>(updatedTransaction);
    }
}