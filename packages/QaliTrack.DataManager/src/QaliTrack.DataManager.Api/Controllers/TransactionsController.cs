using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.DataManager.Core.Common;
using QaliTrack.DataManager.Core.Modules.Transactions.Entities;
using QaliTrack.DataManager.Infrastructure.Data;

namespace QaliTrack.DataManager.Api.Controllers;

[ApiController]
[Route("transactions")]
public class TransactionsController : ControllerBase
{
    private readonly DataManagerDbContext _context;

    public TransactionsController(DataManagerDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<WeighingTransaction>>>> GetTransactions([FromQuery] QueryParameters queryParams)
    {
        var query = _context.WeighingTransactions.AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.Search))
        {
            query = query.Where(t => t.TransactionNumber.Contains(queryParams.Search));
        }

        var totalCount = await query.CountAsync();
        var transactions = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        return Ok(new ApiResponse<IEnumerable<WeighingTransaction>>
        {
            Data = transactions,
            Success = true,
            Message = "Transactions retrieved successfully",
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<WeighingTransaction>>> GetTransaction(Guid id)
    {
        var transaction = await _context.WeighingTransactions.FindAsync(id);
        if (transaction == null)
        {
            return NotFound(new ApiResponse<WeighingTransaction> { Success = false, Message = "Transaction not found" });
        }

        return Ok(new ApiResponse<WeighingTransaction> { Data = transaction, Success = true, Message = "Transaction retrieved successfully" });
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<WeighingTransaction>>> CreateTransaction(WeighingTransaction transaction)
    {
        _context.WeighingTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id },
            new ApiResponse<WeighingTransaction> { Data = transaction, Success = true, Message = "Transaction created successfully" });
    }
}