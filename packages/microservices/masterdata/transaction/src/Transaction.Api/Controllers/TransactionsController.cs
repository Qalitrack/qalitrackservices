// ============================================
// TransactionsController.cs - Updated Controller
// ============================================
using Microsoft.AspNetCore.Mvc;
using Transaction.Core.DTOs;
using Transaction.Core.Interfaces;

namespace Transaction.Api.Controllers;

[Route("Transaction")]
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
    /// Get transaction by ID (TicketID)
    /// </summary>
    [HttpGet("{ticketId}")]
    public async Task<IActionResult> GetById(string ticketId)
    {
        try
        {
            var transaction = await _transactionService.GetByIdAsync(ticketId);
            if (transaction == null)
            {
                return NotFound("Transaction not found");
            }

            return Ok(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction with TicketID {TicketId}", ticketId);
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
    [HttpGet("incomplete/vehicle-id/{vehicleId}")]
    public async Task<IActionResult> GetIncompleteByVehicleId(string vehicleId)
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
    [HttpPost("Transaction")]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var transaction = await _transactionService.CreateAsync(request.Request);
            return CreatedAtAction(nameof(GetById), new { ticketId = transaction.TicketID }, transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transaction");
            return InternalServerError("An error occurred while creating transaction");
        }
    }

    /// <summary>
    /// Add second weight to an existing transaction
    /// </summary>
    [HttpPost("add-second-weight")]
    public async Task<IActionResult> AddSecondWeight([FromBody] AddSecondWeightDto request)
    {
        try
        {
            var transaction = await _transactionService.AddSecondWeightAsync(request);
            if (transaction == null)
            {
                return NotFound("Transaction not found");
            }

            return Ok(transaction, "Second weight added successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when adding second weight to transaction {TicketID}", request.TicketID);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding second weight to transaction {TicketID}", request.TicketID);
            return InternalServerError("An error occurred while adding second weight");
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
            _logger.LogWarning(ex, "Invalid operation when completing transaction {TicketID}", request.TicketID);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing transaction {TicketID}", request.TicketID);
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
            _logger.LogWarning(ex, "Invalid operation when requesting reweigh for transaction {TicketID}", request.TicketID);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting reweigh for transaction {TicketID}", request.TicketID);
            return InternalServerError("An error occurred while requesting reweigh");
        }
    }

    /// <summary>
    /// Get reweigh records for a transaction
    /// </summary>
    [HttpGet("{ticketId}/reweigh-records")]
    public async Task<IActionResult> GetReweighRecords(string ticketId)
    {
        try
        {
            var records = await _transactionService.GetReweighRecordsAsync(ticketId);
            return Ok(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reweigh records for transaction {TicketID}", ticketId);
            return InternalServerError("An error occurred while retrieving reweigh records");
        }
    }

    /// <summary>
    /// Update an existing transaction
    /// </summary>
    [HttpPut("{ticketId}")]
    public async Task<IActionResult> Update(string ticketId, [FromBody] UpdateTransactionDto request)
    {
        try
        {
            var transaction = await _transactionService.UpdateAsync(ticketId, request);
            if (transaction == null)
            {
                return NotFound("Transaction not found");
            }

            return Ok(transaction, "Transaction updated successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when updating transaction {TicketID}", ticketId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating transaction with TicketID {TicketID}", ticketId);
            return InternalServerError("An error occurred while updating transaction");
        }
    }

    /// <summary>
    /// Delete a transaction
    /// </summary>
    [HttpDelete("{ticketId}")]
    public async Task<IActionResult> Delete(string ticketId)
    {
        try
        {
            var result = await _transactionService.DeleteAsync(ticketId);
            if (!result)
            {
                return NotFound("Transaction not found");
            }

            return Ok<object?>(null, "Transaction deleted successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when deleting transaction {TicketID}", ticketId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting transaction with TicketID {TicketID}", ticketId);
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

// Wrapper class for the request with nested structure
public class CreateTransactionRequest
{
    public CreateTransactionDto Request { get; set; } = new();
}