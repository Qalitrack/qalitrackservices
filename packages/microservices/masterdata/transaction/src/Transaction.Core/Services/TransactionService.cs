using AutoMapper;
using Transaction.Core.DTOs;
using Transaction.Core.Entities;
using Transaction.Core.Interfaces;

namespace Transaction.Core.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMapper _mapper;
    private readonly ITimeService _timeService;
    private readonly IReceiptNumberService _receiptNumberService;

    public TransactionService(
        ITransactionRepository transactionRepository, 
        IMapper mapper, 
        ITimeService timeService,
        IReceiptNumberService receiptNumberService)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _timeService = timeService ?? throw new ArgumentNullException(nameof(timeService));
        _receiptNumberService = receiptNumberService ?? throw new ArgumentNullException(nameof(receiptNumberService));
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

    public async Task<TransactionReadDto> CreateAsync(CreateTransactionDto dto)
    {
        var utcNow = _timeService.UtcNow;
        
        // Generate receipt number
        var receiptNo = await _receiptNumberService.GenerateReceiptNumberAsync();
        
        var transaction = _mapper.Map<WeighbridgeTransaction>(dto);
        
        // Generate GUID for TicketID if not provided
        if (string.IsNullOrEmpty(transaction.TicketID))
        {
            transaction.TicketID = Guid.NewGuid().ToString();
        }
        
        transaction.ReceiptNo = receiptNo;
        transaction.CreatedAt = utcNow;
        transaction.UpdatedAt = utcNow;
        transaction.Status = "Active";
        transaction.FirstWeightDate = utcNow;
        transaction.SecondWeightDate = utcNow;
        
        var createdTransaction = await _transactionRepository.CreateAsync(transaction);
        return _mapper.Map<TransactionReadDto>(createdTransaction);
    }

    public async Task<TransactionReadDto?> UpdateAsync(string ticketId, UpdateTransactionDto dto)
    {
        var existingTransaction = await _transactionRepository.GetByIdAsync(ticketId);
        if (existingTransaction == null)
        {
            return null;
        }

        var utcNow = _timeService.UtcNow;
        
        // Only update non-null fields
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
        
        existingTransaction.UpdatedAt = utcNow;
        existingTransaction.ChangeDate = utcNow;
        
        var updatedTransaction = await _transactionRepository.UpdateAsync(existingTransaction);
        return updatedTransaction == null ? null : _mapper.Map<TransactionReadDto>(updatedTransaction);
    }

    public async Task<bool> DeleteAsync(string ticketId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(ticketId);
        if (transaction == null)
        {
            return false;
        }

        // Check if transaction is completed
        if (transaction.Status == "Completed")
        {
            throw new InvalidOperationException("Cannot delete a completed transaction.");
        }

        return await _transactionRepository.DeleteAsync(ticketId);
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

        // Check if second weight already exists
        if (!string.IsNullOrEmpty(transaction.SecondWeight))
        {
            throw new InvalidOperationException("Second weight has already been recorded for this transaction.");
        }

        var utcNow = _timeService.UtcNow;

        // Update second weighing information
        transaction.SecondWeight = dto.SecondWeight;
        transaction.WeighBridgeName2nd = dto.WeighBridgeName2nd;
        transaction.ScaleName2nd = dto.ScaleName2nd;
        transaction.OperatorID2nd = dto.OperatorID2nd;
        transaction.OperatorName2nd = dto.OperatorName2nd;
        transaction.SecondWeightDate = utcNow;
        transaction.UpdatedAt = utcNow;

        // Calculate net weight
        if (decimal.TryParse(transaction.FirstWeight, out var firstWeight) && 
            decimal.TryParse(transaction.SecondWeight, out var secondWeight))
        {
            var netWeight = Math.Abs(firstWeight - secondWeight);
            transaction.NetWeight = netWeight.ToString("F2");
        }

        // Update status to completed if both weights are present
        if (!string.IsNullOrEmpty(transaction.FirstWeight) && !string.IsNullOrEmpty(transaction.SecondWeight))
        {
            transaction.Status = "Completed";
        }

        var updatedTransaction = await _transactionRepository.UpdateAsync(transaction);
        return updatedTransaction == null ? null : _mapper.Map<TransactionReadDto>(updatedTransaction);
    }

    public async Task<TransactionReadDto?> CompleteTransactionAsync(CompleteTransactionDto dto)
    {
        var transaction = await _transactionRepository.GetByIdAsync(dto.TicketID);
        if (transaction == null)
        {
            throw new ArgumentException("Transaction not found", nameof(dto.TicketID));
        }

        // Verify both weights are present
        if (string.IsNullOrEmpty(transaction.FirstWeight) || string.IsNullOrEmpty(transaction.SecondWeight))
        {
            throw new InvalidOperationException("Cannot complete transaction. Both first and second weights are required.");
        }

        var utcNow = _timeService.UtcNow;
        
        // Calculate net weight if not already calculated
        if (string.IsNullOrEmpty(transaction.NetWeight))
        {
            if (decimal.TryParse(transaction.FirstWeight, out var firstWeight) && 
                decimal.TryParse(transaction.SecondWeight, out var secondWeight))
            {
                var netWeight = Math.Abs(firstWeight - secondWeight);
                transaction.NetWeight = netWeight.ToString("F2");
            }
        }

        transaction.Status = "Completed";
        transaction.UpdatedAt = utcNow;

        var updatedTransaction = await _transactionRepository.UpdateAsync(transaction);
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

        if (transaction.Status != "Completed")
        {
            throw new InvalidOperationException("Can only request reweigh for completed transactions.");
        }

        var utcNow = _timeService.UtcNow;

        // Update reweigh permission
        transaction.ReweighPermission = dto.Reason;
        transaction.Status = "ReweighRequested";
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
}