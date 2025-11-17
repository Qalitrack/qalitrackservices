using AutoMapper;
using Transaction.Core.DTOs;
using Transaction.Core.Entities;
using Transaction.Core.Interfaces;
using AddWeighingDto = Transaction.Core.DTOs.AddWeighingDto;

namespace Transaction.Core.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMapper _mapper;
    private readonly ITimeService _timeService;

    public TransactionService(ITransactionRepository transactionRepository, IMapper mapper, ITimeService timeService)
    {
        _transactionRepository = transactionRepository;
        _mapper = mapper;
        _timeService = timeService;
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

    public async Task<TransactionReadDto?> GetByIdAsync(string id)
    {
        // Use GetWithWeighingRecordsAsync to ensure related entities are loaded
        var transaction = await _transactionRepository.GetWithWeighingRecordsAsync(id);
        return transaction == null ? null : _mapper.Map<TransactionReadDto>(transaction);
    }

    public async Task<TransactionReadDto?> GetByReceiptNoAsync(string receiptNo)
    {
        var transaction = await _transactionRepository.GetByReceiptNoAsync(receiptNo);
        return transaction == null ? null : _mapper.Map<TransactionReadDto>(transaction);
    }

    public async Task<TransactionReadDto> CreateAsync(CreateTransactionDto dto)
    {
        var transaction = _mapper.Map<WeighbridgeTransaction>(dto);
        transaction.CreatedAt = _timeService.Now;
        transaction.UpdatedAt = _timeService.Now;
        transaction.Status = WeighbridgeTransactionStatus.Pending;
        transaction.IsCompleted = false;
        
        // If first weight is provided, set it
        if (dto.FirstWeight.HasValue)
        {
            transaction.FirstWeight = dto.FirstWeight.Value;
            transaction.FirstWeightTimestamp = _timeService.Now;
            transaction.CompletedWeighings = 1;
            transaction.Status = WeighbridgeTransactionStatus.InProgress;
            
            // Create first weighing record
            var weighingRecord = new WeighingRecord
            {
                WeighingSequence = 1,
                Weight = dto.FirstWeight.Value,
                WeighingDate = _timeService.Now,
                WeighBridgeId = dto.WeighBridgeId,
                WeighBridgeName = dto.WeighBridgeName,
                ScaleName = dto.ScaleName,
                OperatorId = dto.OperatorId,
                OperatorName = dto.OperatorName,
                CreatedAt = _timeService.Now,
                UpdatedAt = _timeService.Now
            };
            transaction.WeighingRecords.Add(weighingRecord);
        }
        
        // Create audit log
        var auditLog = new TransactionAuditLog
        {
            Action = "Created",
            ChangedBy = dto.OperatorName ?? "System",
            ChangeTimestamp = _timeService.Now,
            NewValues = System.Text.Json.JsonSerializer.Serialize(dto),
            Reason = "New transaction created",
            CreatedAt = _timeService.Now,
            UpdatedAt = _timeService.Now
        };
        transaction.AuditLogs.Add(auditLog);
        
        var createdTransaction = await _transactionRepository.CreateAsync(transaction);
        return _mapper.Map<TransactionReadDto>(createdTransaction);
    }

    public async Task<TransactionReadDto?> UpdateAsync(string id, UpdateTransactionDto dto)
    {
        var existingTransaction = await _transactionRepository.GetByIdAsync(id.ToString());
        if (existingTransaction == null)
        {
            return null;
        }

        // Check if transaction is completed (immutable)
        if (!existingTransaction.CanBeModified())
        {
            throw new InvalidOperationException("Cannot modify a completed transaction. Request reweigh permission if needed.");
        }

        var oldValues = System.Text.Json.JsonSerializer.Serialize(existingTransaction);
        
        // Only update non-null fields
        if (dto.NoPlate != null) existingTransaction.NoPlate = dto.NoPlate;
        if (dto.DriverName != null) existingTransaction.DriverName = dto.DriverName;
        if (dto.VehicleId.HasValue) existingTransaction.VehicleId = dto.VehicleId;
        if (dto.ProductId.HasValue) existingTransaction.CommodityId = dto.ProductId;
        if (dto.ProductName != null) existingTransaction.CommodityName = dto.ProductName;
        if (dto.SupplierId.HasValue) existingTransaction.SupplierId = dto.SupplierId;
        if (dto.SupplierName != null) existingTransaction.SupplierName = dto.SupplierName;
        if (dto.CustomerId.HasValue) existingTransaction.CustomerId = dto.CustomerId;
        if (dto.CustomerName != null) existingTransaction.CustomerName = dto.CustomerName;
        if (dto.TransporterId.HasValue) existingTransaction.TransporterId = dto.TransporterId.Value;
        if (dto.TransporterName != null) existingTransaction.TransporterName = dto.TransporterName;
        if (dto.OriginId.HasValue) existingTransaction.OriginId = dto.OriginId;
        if (dto.OriginName != null) existingTransaction.OriginName = dto.OriginName;
        if (dto.DestinationId.HasValue) existingTransaction.DestinationId = dto.DestinationId;
        if (dto.DestinationName != null) existingTransaction.DestinationName = dto.DestinationName;
        if (dto.WeighMode != null) existingTransaction.WeighMode = dto.WeighMode;
        if (dto.Operation != null) existingTransaction.Operation = dto.Operation;
        if (dto.ChangeDescription != null) existingTransaction.ChangeDescription = dto.ChangeDescription;
        
        existingTransaction.UpdatedAt = _timeService.Now;
        existingTransaction.ChangeDate = _timeService.Now;
        
        // Create audit log
        var auditLog = new TransactionAuditLog
        {
            WeighbridgeTransactionId = existingTransaction.Id,
            Action = "Updated",
            ChangedBy = "System", // TODO: Get from current user context
            OldValues = oldValues,
            NewValues = System.Text.Json.JsonSerializer.Serialize(existingTransaction),
            ChangeTimestamp = _timeService.Now,
            Reason = dto.ChangeDescription ?? "Transaction updated",
            CreatedAt = _timeService.Now,
            UpdatedAt = _timeService.Now
        };
        existingTransaction.AuditLogs.Add(auditLog);
        
        var updatedTransaction = await _transactionRepository.UpdateAsync(existingTransaction);
        return updatedTransaction == null ? null : _mapper.Map<TransactionReadDto>(updatedTransaction);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id.ToString());
        if (transaction == null)
        {
            return false;
        }

        // Check if transaction is completed
        if (transaction.IsCompleted)
        {
            throw new InvalidOperationException("Cannot delete a completed transaction.");
        }

        return await _transactionRepository.DeleteAsync(id.ToString());
    }

    public async Task<bool> IsReceiptNoAvailableAsync(string receiptNo)
    {
        return await _transactionRepository.IsReceiptNoAvailableAsync(receiptNo);
    }

    public async Task<TransactionReadDto?> AddWeighingAsync(AddWeighingDto dto)
    {
        var transaction = await _transactionRepository.GetWithWeighingRecordsAsync(dto.TransactionId);
        if (transaction == null)
        {
            return null;
        }

        // Check if we can add more weighings
        if (!transaction.CanAddWeighing())
        {
            throw new InvalidOperationException("Cannot add more weighings. Transaction is either completed or has reached maximum weighings.");
        }
        
        // Ensure the transaction is in a valid state for adding weighings
        if (transaction.Status == WeighbridgeTransactionStatus.Pending)
        {
            transaction.Status = WeighbridgeTransactionStatus.InProgress;
        }

        // The sequence number is the next weighing number (1-based index)
        var sequenceNumber = (transaction.WeighingRecords?.Count ?? 0) + 1;

        // Add weight based on sequence
        if (sequenceNumber == 1)
        {
            transaction.FirstWeight = dto.Weight;
            transaction.FirstWeightTimestamp = _timeService.Now;
            transaction.WeighBridgeId = dto.WeighBridgeId;
            transaction.WeighBridgeName = dto.WeighBridgeName;
            transaction.ScaleName = dto.ScaleName;
            transaction.OperatorId = dto.OperatorId;
            transaction.OperatorName = dto.OperatorName;
            transaction.Status = WeighbridgeTransactionStatus.InProgress;
        }
        else if (sequenceNumber == 2)
        {
            transaction.SecondWeight = dto.Weight;
            transaction.SecondWeightTimestamp = _timeService.Now;
            transaction.WeighBridgeName2nd = dto.WeighBridgeName;
            transaction.ScaleName2nd = dto.ScaleName;
            transaction.OperatorId2nd = dto.OperatorId;
            transaction.OperatorName2nd = dto.OperatorName;
        }

        // Create and add weighing record to the transaction
        var weighingRecord = new WeighingRecord
        {
            WeighbridgeTransactionId = transaction.Id,
            WeighingSequence = sequenceNumber,
            Weight = dto.Weight,
            WeighingDate = _timeService.Now,
            WeighBridgeId = dto.WeighBridgeId,
            WeighBridgeName = dto.WeighBridgeName,
            ScaleName = dto.ScaleName,
            OperatorId = dto.OperatorId,
            OperatorName = dto.OperatorName,
            Notes = dto.Notes ?? string.Empty,
            CreatedAt = _timeService.Now,
            UpdatedAt = _timeService.Now
        };
        
        // Ensure WeighingRecords collection is initialized
        if (transaction.WeighingRecords == null)
        {
            transaction.WeighingRecords = new List<WeighingRecord>();
        }
        
        // Add the weighing record to the transaction
        transaction.WeighingRecords.Add(weighingRecord);
        
        // Update the CompletedWeighings to match the actual count of weighings
        transaction.CompletedWeighings = transaction.WeighingRecords.Count;
        
        // Debug: Log current state before updating status
        Console.WriteLine($"DEBUG - Sequence: {sequenceNumber}, Expected: {transaction.ExpectedWeighings}, Current Status: {transaction.Status}");
        
        // Ensure CompletedWeighings is properly set
        transaction.CompletedWeighings = sequenceNumber;
        
        // Update the transaction's status based on the weighing sequence
        if (sequenceNumber >= transaction.ExpectedWeighings)
        {
            // Transaction is complete
            transaction.Status = WeighbridgeTransactionStatus.Completed;
            transaction.IsCompleted = true;
            transaction.CompletedDate = _timeService.Now;
            Console.WriteLine($"DEBUG - Setting status to Completed. Sequence: {sequenceNumber}, Expected: {transaction.ExpectedWeighings}");
            
            // For transactions with exactly 2 weighings, calculate net weight as first - second
            if (transaction.ExpectedWeighings == 2 && transaction.WeighingRecords.Count == 2)
            {
                var firstWeighing = transaction.WeighingRecords.OrderBy(w => w.WeighingSequence).First();
                var secondWeighing = transaction.WeighingRecords.OrderBy(w => w.WeighingSequence).Last();
                transaction.NetWeight = Math.Abs(firstWeighing.Weight - secondWeighing.Weight);
                transaction.NetWeightCalculatedTimestamp = _timeService.Now;
            }
            // For transactions with more than 2 weighings, calculate net weight as first - last
            else if (transaction.ExpectedWeighings > 2 && transaction.WeighingRecords.Count >= 2)
            {
                var firstWeighing = transaction.WeighingRecords.OrderBy(w => w.WeighingSequence).First();
                var lastWeighing = transaction.WeighingRecords.OrderByDescending(w => w.WeighingSequence).First();
                transaction.NetWeight = Math.Abs(firstWeighing.Weight - lastWeighing.Weight);
                transaction.NetWeightCalculatedTimestamp = _timeService.Now;
            }
        }
        // For transactions that are in progress but not yet complete
        else if (sequenceNumber > 0)
        {
            transaction.Status = WeighbridgeTransactionStatus.InProgress;
        }
        
        // Update the transaction's updated timestamp
        transaction.UpdatedAt = _timeService.Now;
        
        // Debug: Log transaction state before saving
        Console.WriteLine($"DEBUG - Before Save - Status: {transaction.Status}, IsCompleted: {transaction.IsCompleted}, WeighingRecords: {transaction.WeighingRecords?.Count ?? 0}, CompletedWeighings: {transaction.CompletedWeighings}");
        
        // Mark the entity as modified to ensure all properties are updated
        _transactionRepository.MarkAsModified(transaction);
        
        // Save the transaction
        var savedTransaction = await _transactionRepository.UpdateAsync(transaction);
        if (savedTransaction == null) 
        {
            Console.WriteLine("DEBUG - Failed to save transaction");
            return null;
        }
        
        // Debug: Log saved transaction state
        Console.WriteLine($"DEBUG - After Save - Status: {savedTransaction.Status}, IsCompleted: {savedTransaction.IsCompleted}, WeighingRecords: {savedTransaction.WeighingRecords?.Count ?? 0}");
        
        // Create audit log
        var auditLog = new TransactionAuditLog
        {
            WeighbridgeTransactionId = savedTransaction.Id,
            Action = $"Weighing{sequenceNumber}Added",
            ChangedBy = dto.OperatorName,
            NewValues = System.Text.Json.JsonSerializer.Serialize(dto),
            ChangeTimestamp = _timeService.Now,
            Reason = $"Weighing #{sequenceNumber} completed",
            CreatedAt = _timeService.Now,
            UpdatedAt = _timeService.Now
        };
        
        if (savedTransaction.AuditLogs == null)
        {
            savedTransaction.AuditLogs = new List<TransactionAuditLog>();
        }
        savedTransaction.AuditLogs.Add(auditLog);
        
        // Save the transaction with the audit log
        savedTransaction = await _transactionRepository.UpdateAsync(savedTransaction);
        
        // Ensure we have the latest state from the database with all related entities
        var result = await _transactionRepository.GetWithWeighingRecordsAsync(savedTransaction.Id);
        
        // Use AutoMapper for consistent mapping
        return _mapper.Map<TransactionReadDto>(result);
    }

    public async Task<TransactionReadDto?> CompleteTransactionAsync(CompleteTransactionDto dto)
    {
        var transaction = await _transactionRepository.GetWithWeighingRecordsAsync(dto.TransactionId);
        if (transaction == null)
        {
            throw new ArgumentException("Transaction not found", nameof(dto.TransactionId));
        }

        if (transaction.IsReweighInProgress)
        {
            transaction.CompleteReweigh(_timeService.Now);
        }
        else
        {
            if (transaction.CompletedWeighings < transaction.ExpectedWeighings)
            {
                throw new InvalidOperationException($"Cannot complete transaction. Expected {transaction.ExpectedWeighings} weighings but only {transaction.CompletedWeighings} completed.");
            }
            transaction.CompleteTransaction(_timeService.Now);
        }
    
        // Create audit log
        var auditLog = new TransactionAuditLog
        {
            WeighbridgeTransactionId = transaction.Id,
            Action = transaction.IsReweighInProgress ? "ReweighCompleted" : "Completed",
            ChangedBy = "System", // TODO: Get from current user context
            ChangeTimestamp = _timeService.Now,
            Reason = transaction.IsReweighInProgress ? "Reweigh completed" : "Transaction completed",
            CreatedAt = _timeService.Now,
            UpdatedAt = _timeService.Now
        };
        transaction.AuditLogs.Add(auditLog);

        var updatedTransaction = await _transactionRepository.UpdateAsync(transaction);
        return updatedTransaction == null ? null : _mapper.Map<TransactionReadDto>(updatedTransaction);
    }

    public async Task<TransactionReadDto?> StartReweighAsync(string transactionId, string startedBy)
    {
        var transaction = await _transactionRepository.GetWithWeighingRecordsAsync(transactionId);
        if (transaction == null)
        {
            throw new ArgumentException("Transaction not found", nameof(transactionId));
        }

        if (!transaction.IsReweighRequested)
        {
            throw new InvalidOperationException("Cannot start reweigh that hasn't been requested.");
        }

        if (transaction.IsReweighInProgress)
        {
            throw new InvalidOperationException("Reweigh is already in progress for this transaction.");
        }

        transaction.StartReweigh(_timeService.Now);
        transaction.UpdatedAt = _timeService.Now;

        // Create audit log
        var auditLog = new TransactionAuditLog
        {
            WeighbridgeTransactionId = transactionId.ToString(),
            Action = "ReweighStarted",
            ChangedBy = startedBy,
            ChangeTimestamp = _timeService.Now,
            Reason = "Starting reweigh process",
            CreatedAt = _timeService.Now,
            UpdatedAt = _timeService.Now
        };
        transaction.AuditLogs.Add(auditLog);

        var updatedTransaction = await _transactionRepository.UpdateAsync(transaction);
        return updatedTransaction == null ? null : _mapper.Map<TransactionReadDto>(updatedTransaction);
    }

    public async Task<TransactionReadDto> AddReweighWeightAsync(AddReweighWeightDto dto)
    {
        var transaction = await _transactionRepository.GetWithWeighingRecordsAsync(dto.TransactionId);
        if (transaction == null)
        {
            throw new ArgumentException("Transaction not found", nameof(dto.TransactionId));
        }

        if (!transaction.IsReweighInProgress)
        {
            throw new InvalidOperationException("Cannot add weight - no reweigh in progress for this transaction");
        }

        // Add the weight to the current reweigh attempt
        transaction.AddReweighWeight(dto.Weight, dto.OperatorName, _timeService.Now, dto.Notes);
        
        // Create audit log
        var auditLog = new TransactionAuditLog
        {
            WeighbridgeTransactionId = dto.TransactionId,
            Action = "ReweighWeightAdded",
            ChangedBy = dto.OperatorName,
            ChangeTimestamp = _timeService.Now,
            Reason = $"Added weight {dto.Weight} to reweigh attempt {transaction.CurrentReweighAttempt}",
            CreatedAt = _timeService.Now,
            UpdatedAt = _timeService.Now
        };
        transaction.AuditLogs.Add(auditLog);

        var updatedTransaction = await _transactionRepository.UpdateAsync(transaction);
        return _mapper.Map<TransactionReadDto>(updatedTransaction);
    }

    public async Task<TransactionReadDto> CompleteReweighAsync(CompleteReweighDto dto)
    {
        var transaction = await _transactionRepository.GetWithWeighingRecordsAsync(dto.TransactionId);
        if (transaction == null)
        {
            throw new ArgumentException("Transaction not found", nameof(dto.TransactionId));
        }

        if (!transaction.IsReweighInProgress)
        {
            throw new InvalidOperationException("Cannot complete reweigh - no reweigh in progress for this transaction");
        }

        // Complete the current reweigh attempt
        transaction.CompleteReweigh(_timeService.Now);
        
        // Create audit log
        var auditLog = new TransactionAuditLog
        {
            WeighbridgeTransactionId = dto.TransactionId,
            Action = "ReweighCompleted",
            ChangedBy = dto.CompletedBy,
            ChangeTimestamp = _timeService.Now,
            Reason = $"Reweigh attempt {transaction.CurrentReweighAttempt} completed" + (string.IsNullOrEmpty(dto.Notes) ? "" : $": {dto.Notes}"),
            CreatedAt = _timeService.Now,
            UpdatedAt = _timeService.Now
        };
        transaction.AuditLogs.Add(auditLog);

        var updatedTransaction = await _transactionRepository.UpdateAsync(transaction);
        return _mapper.Map<TransactionReadDto>(updatedTransaction);
    }

    public async Task<IEnumerable<ReweighRecordDto>> GetReweighRecordsAsync(string transactionId)
    {
        var transaction = await _transactionRepository.GetWithWeighingRecordsAsync(transactionId);
        if (transaction == null)
        {
            throw new ArgumentException("Transaction not found", nameof(transactionId));
        }

        return _mapper.Map<IEnumerable<ReweighRecordDto>>(transaction.ReweighRecords ?? new List<ReweighRecord>());
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
        if (!Enum.TryParse<WeighbridgeTransactionStatus>(status, true, out var statusEnum))
        {
            throw new ArgumentException($"Invalid status: {status}");
        }

        var transactions = await _transactionRepository.GetTransactionsByStatusAsync(statusEnum, limit);
        return _mapper.Map<IEnumerable<TransactionReadDto>>(transactions);
    }

    public async Task<TransactionReadDto?> GetWithWeighingRecordsAsync(string id)
    {
        var transaction = await _transactionRepository.GetWithWeighingRecordsAsync(id);
        return transaction == null ? null : _mapper.Map<TransactionReadDto>(transaction);
    }

    public async Task<TransactionReadDto?> GetWithAuditLogsAsync(string id)
    {
        var transaction = await _transactionRepository.GetWithAuditLogsAsync(id);
        return transaction == null ? null : _mapper.Map<TransactionReadDto>(transaction);
    }

    public async Task<bool> RequestReweighAsync(RequestReweighDto dto)
    {
        var transaction = await _transactionRepository.GetByIdAsync(dto.TransactionId);
        if (transaction == null)
        {
            throw new ArgumentException("Transaction not found", nameof(dto.TransactionId));
        }

        if (!transaction.IsCompleted)
        {
            throw new InvalidOperationException("Can only request reweigh for completed transactions.");
        }

        if (transaction.IsReweighRequested || transaction.IsReweighInProgress)
        {
            throw new InvalidOperationException("A reweigh has already been requested or is in progress for this transaction.");
        }

        transaction.RequestReweigh(dto.Reason, "System", _timeService.Now); // TODO: Replace "System" with actual user

        // Create audit log
        var auditLog = new TransactionAuditLog
        {
            WeighbridgeTransactionId = transaction.Id,
            Action = "ReweighRequested",
            ChangedBy = "System", // TODO: Get from current user context
            ChangeTimestamp = _timeService.Now,
            Reason = dto.Reason,
            CreatedAt = _timeService.Now,
            UpdatedAt = _timeService.Now
        };
        transaction.AuditLogs.Add(auditLog);

        await _transactionRepository.UpdateAsync(transaction);
        return true;
    }
}