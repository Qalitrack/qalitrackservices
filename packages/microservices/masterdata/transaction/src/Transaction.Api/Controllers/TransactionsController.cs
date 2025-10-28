using Microsoft.AspNetCore.Mvc;
using Transaction.Core.DTOs;
using Transaction.Core.Interfaces;

namespace Transaction.Api.Controllers;

[Route("[controller]")]
public class TransactionsController : BaseController
{
    private readonly ITransactionService _transactionService;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(ITransactionService transactionService, ILogger<TransactionsController> logger)
    {
        _transactionService = transactionService;
        _logger = logger;
    }

    /// <summary>
    /// Get all transactions with pagination and filters
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] WeighbridgeTransactionFilter filter)
    {
        try
        {
            var pagedTransactions = await _transactionService.GetAllAsync(filter);
            return Ok(pagedTransactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all transactions");
            return InternalServerError("An error occurred while retrieving transactions");
        }
    }

    /// <summary>
    /// Get transaction by ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var transaction = await _transactionService.GetByIdAsync(id);
            if (transaction == null)
            {
                return NotFound("Transaction not found");
            }

            return Ok(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction with id {Id}", id);
            return InternalServerError("An error occurred while retrieving transaction");
        }
    }

    /// <summary>
    /// Get transaction by receipt number
    /// </summary>
    [HttpGet("receipt/{receiptNo}")]
    public async Task<IActionResult> GetByReceiptNo(string receiptNo)
    {
        try
        {
            var transaction = await _transactionService.GetByReceiptNoAsync(receiptNo);
            if (transaction == null)
            {
                return NotFound("Transaction not found");
            }

            return Ok(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction with receipt number {ReceiptNo}", receiptNo);
            return InternalServerError("An error occurred while retrieving transaction");
        }
    }

    /// <summary>
    /// Get transaction with weighing records
    /// </summary>
    [HttpGet("{id}/weighing-records")]
    public async Task<IActionResult> GetWithWeighingRecords(string id)
    {
        try
        {
            var transaction = await _transactionService.GetWithWeighingRecordsAsync(id);
            if (transaction == null)
            {
                return NotFound("Transaction not found");
            }

            return Ok(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction with weighing records for id {Id}", id);
            return InternalServerError("An error occurred while retrieving transaction");
        }
    }

    /// <summary>
    /// Get transaction with audit logs
    /// </summary>
    [HttpGet("{id}/audit-logs")]
    public async Task<IActionResult> GetWithAuditLogs(string id)
    {
        try
        {
            var transaction = await _transactionService.GetWithAuditLogsAsync(id);
            if (transaction == null)
            {
                return NotFound("Transaction not found");
            }

            return Ok(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction with audit logs for id {Id}", id);
            return InternalServerError("An error occurred while retrieving transaction");
        }
    }

    /// <summary>
    /// Get incomplete transactions by vehicle number plate
    /// </summary>
    [HttpGet("incomplete/vehicle/{noPlate}")]
    public async Task<IActionResult> GetIncompleteByVehicle(string noPlate)
    {
        try
        {
            var transactions = await _transactionService.GetIncompleteTransactionsByVehicleAsync(noPlate);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting incomplete transactions for vehicle {NoPlate}", noPlate);
            return InternalServerError("An error occurred while retrieving transactions");
        }
    }

    /// <summary>
    /// Get incomplete transactions by vehicle ID
    /// </summary>
    [HttpGet("incomplete/vehicle-id/{vehicleId:int}")]
    public async Task<IActionResult> GetIncompleteByVehicleId(int vehicleId)
    {
        try
        {
            var transactions = await _transactionService.GetIncompleteTransactionsByVehicleIdAsync(vehicleId);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting incomplete transactions for vehicle id {VehicleId}", vehicleId);
            return InternalServerError("An error occurred while retrieving transactions");
        }
    }

    /// <summary>
    /// Get transactions by status
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(string status, [FromQuery] int limit = 100)
    {
        try
        {
            var transactions = await _transactionService.GetTransactionsByStatusAsync(status, limit);
            return Ok(transactions);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid status provided: {Status}", status);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transactions by status {Status}", status);
            return InternalServerError("An error occurred while retrieving transactions");
        }
    }

    /// <summary>
    /// Create a new transaction
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionDto request)
    {
        try
        {
            var transaction = await _transactionService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transaction");
            return InternalServerError("An error occurred while creating transaction");
        }
    }

    /// <summary>
    /// Add a weighing to an existing transaction
    /// </summary>
    [HttpPost("add-weighing")]
    public async Task<IActionResult> AddWeighing([FromBody] AddWeighingDto request)
    {
        try
        {
            var transaction = await _transactionService.AddWeighingAsync(request);
            if (transaction == null)
            {
                return NotFound("Transaction not found");
            }

            return Ok(transaction, "Weighing added successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when adding weighing to transaction {TransactionId}", request.TransactionId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding weighing to transaction {TransactionId}", request.TransactionId);
            return InternalServerError("An error occurred while adding weighing");
        }
    }

    /// <summary>
    /// Complete a transaction
    /// </summary>
    [HttpPost("complete")]
    public async Task<IActionResult> Complete([FromBody] CompleteTransactionDto request)
    {
        try
        {
            var transaction = await _transactionService.CompleteTransactionAsync(request);
            if (transaction == null)
            {
                return NotFound("Transaction not found");
            }

            return Ok(transaction, "Transaction completed successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when completing transaction {TransactionId}", request.TransactionId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing transaction {TransactionId}", request.TransactionId);
            return InternalServerError("An error occurred while completing transaction");
        }
    }

    /// <summary>
    /// Request reweigh permission for a completed transaction
    /// </summary>
    [HttpPost("request-reweigh")]
    public async Task<IActionResult> RequestReweigh([FromBody] RequestReweighDto request)
    {
        try
        {
            var result = await _transactionService.RequestReweighAsync(request);
            if (!result)
            {
                return NotFound("Transaction not found");
            }

            return Ok<object?>(null, "Reweigh permission requested successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when requesting reweigh for transaction {TransactionId}", request.TransactionId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting reweigh for transaction {TransactionId}", request.TransactionId);
            return InternalServerError("An error occurred while requesting reweigh");
        }
    }

    /// <summary>
    /// Start a reweigh process for a transaction
    /// </summary>
    [HttpPost("{transactionId}/start-reweigh")]
    public async Task<IActionResult> StartReweigh(string transactionId, [FromBody] StartReweighDto dto)
    {
        try
        {
            var result = await _transactionService.StartReweighAsync(transactionId, dto.StartedBy);
            if (result == null)
            {
                return NotFound("Transaction not found");
            }

            return Ok(result, "Reweigh started successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot start reweigh for transaction {TransactionId}", transactionId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting reweigh for transaction {TransactionId}", transactionId);
            return InternalServerError("An error occurred while starting reweigh");
        }
    }

    /// <summary>
    /// Add a weight measurement during reweigh
    /// </summary>
    [HttpPost("add-reweigh-weight")]
    public async Task<IActionResult> AddReweighWeight([FromBody] AddReweighWeightDto dto)
    {
        try
        {
            var result = await _transactionService.AddReweighWeightAsync(dto);
            return Ok(result, "Weight added to reweigh successfully");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid arguments when adding reweigh weight for transaction {TransactionId}", dto.TransactionId);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot add weight to reweigh for transaction {TransactionId}", dto.TransactionId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding weight to reweigh for transaction {TransactionId}", dto.TransactionId);
            return InternalServerError("An error occurred while adding weight to reweigh");
        }
    }

    /// <summary>
    /// Complete a reweigh process
    /// </summary>
    [HttpPost("complete-reweigh")]
    public async Task<IActionResult> CompleteReweigh([FromBody] CompleteReweighDto dto)
    {
        try
        {
            var result = await _transactionService.CompleteReweighAsync(dto);
            return Ok(result, "Reweigh completed successfully");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid arguments when completing reweigh for transaction {TransactionId}", dto.TransactionId);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot complete reweigh for transaction {TransactionId}", dto.TransactionId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing reweigh for transaction {TransactionId}", dto.TransactionId);
            return InternalServerError("An error occurred while completing reweigh");
        }
    }

    /// <summary>
    /// Get reweigh records for a transaction
    /// </summary>
    [HttpGet("{transactionId}/reweigh-records")]
    public async Task<IActionResult> GetReweighRecords(string transactionId)
    {
        try
        {
            var records = await _transactionService.GetReweighRecordsAsync(transactionId);
            return Ok(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reweigh records for transaction {TransactionId}", transactionId);
            return InternalServerError("An error occurred while retrieving reweigh records");
        }
    }

    /// <summary>
    /// Update an existing transaction (only if not completed)
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTransactionDto request)
    {
        try
        {
            var transaction = await _transactionService.UpdateAsync(id, request);
            if (transaction == null)
            {
                return NotFound("Transaction not found");
            }

            return Ok(transaction, "Transaction updated successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when updating transaction {Id}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating transaction with id {Id}", id);
            return InternalServerError("An error occurred while updating transaction");
        }
    }

    /// <summary>
    /// Delete a transaction (only if not completed)
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _transactionService.DeleteAsync(id);
            if (!result)
            {
                return NotFound("Transaction not found");
            }

            return Ok<object?>(null, "Transaction deleted successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when deleting transaction {Id}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting transaction with id {Id}", id);
            return InternalServerError("An error occurred while deleting transaction");
        }
    }

    /// <summary>
    /// Check if receipt number is available
    /// </summary>
    [HttpGet("check-receipt/{receiptNo}")]
    public async Task<IActionResult> CheckReceiptNo(string receiptNo)
    {
        try
        {
            var available = await _transactionService.IsReceiptNoAvailableAsync(receiptNo);
            return Ok(new { available }, available ? "Receipt number is available" : "Receipt number is not available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking receipt number availability");
            return InternalServerError("An error occurred while checking receipt number");
        }
    }
}