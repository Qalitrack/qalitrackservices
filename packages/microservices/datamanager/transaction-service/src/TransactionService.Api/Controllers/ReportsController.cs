using Microsoft.AspNetCore.Mvc;
using TransactionService.Core.DTOs;
using TransactionService.Core.Entities;
using TransactionService.Core.Interfaces;

namespace TransactionService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : BaseController
{
    private readonly ITransactionService _transactionService;
    private readonly IChargeService _chargeService;
    private readonly IWorkflowService _workflowService;

    public ReportsController(
        ITransactionService transactionService,
        IChargeService chargeService,
        IWorkflowService workflowService)
    {
        _transactionService = transactionService;
        _chargeService = chargeService;
        _workflowService = workflowService;
    }

    /// <summary>
    /// Get transaction summary report
    /// </summary>
    [HttpGet("transaction-summary")]
    public async Task<IActionResult> GetTransactionSummary(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? organizationId = null)
    {
        try
        {
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today.AddDays(1);

            var transactions = await _transactionService.GetTransactionsByDateRangeAsync(start, end);
            
            if (!string.IsNullOrEmpty(organizationId))
            {
                transactions = transactions.Where(t => t.OrganizationId == organizationId);
            }

            var summary = new TransactionSummaryReport
            {
                TotalTransactions = transactions.Count(),
                CompletedTransactions = transactions.Count(t => t.Status == TransactionStatus.Completed),
                PendingTransactions = transactions.Count(t => t.Status == TransactionStatus.Pending),
                InProgressTransactions = transactions.Count(t => t.Status == TransactionStatus.InProgress),
                CancelledTransactions = transactions.Count(t => t.Status == TransactionStatus.Cancelled),
                TransactionsByType = transactions.GroupBy(t => t.TransactionType)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                TotalWeight = transactions.Where(t => t.NetWeight.HasValue).Sum(t => t.NetWeight.Value),
                AverageWeight = transactions.Where(t => t.NetWeight.HasValue).Any() 
                    ? transactions.Where(t => t.NetWeight.HasValue).Average(t => t.NetWeight.Value) 
                    : 0m,
                StartDate = start,
                EndDate = end
            };

            return Ok(new ApiResponse<TransactionSummaryReport>
            {
                Success = true,
                Message = "Transaction summary generated successfully",
                Data = summary
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get financial report
    /// </summary>
    [HttpGet("financial")]
    public async Task<IActionResult> GetFinancialReport(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today.AddDays(1);

            var transactions = await _transactionService.GetTransactionsByDateRangeAsync(start, end);
            var report = new FinancialReport
            {
                TotalRevenue = 0,
                TotalUnpaid = 0,
                ChargesByType = new Dictionary<string, decimal>(),
                StartDate = start,
                EndDate = end
            };

            foreach (var transaction in transactions)
            {
                var totalAmount = await _chargeService.GetTotalAmountAsync(transaction.Id);
                var unpaidAmount = await _chargeService.GetUnpaidAmountAsync(transaction.Id);
                
                report.TotalRevenue += totalAmount;
                report.TotalUnpaid += unpaidAmount;
            }

            return Ok(new ApiResponse<FinancialReport>
            {
                Success = true,
                Message = "Financial report generated successfully",
                Data = report
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get workflow performance report
    /// </summary>
    [HttpGet("workflow-performance")]
    public async Task<IActionResult> GetWorkflowPerformanceReport()
    {
        try
        {
            var pendingSteps = await _workflowService.GetPendingStepsAsync();
            
            var report = new WorkflowPerformanceReport
            {
                TotalPendingSteps = pendingSteps.Count(),
                StepsByStatus = pendingSteps.GroupBy(s => s.WorkflowStep)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                AverageProcessingTime = new Dictionary<string, double>()
            };

            return Ok(new ApiResponse<WorkflowPerformanceReport>
            {
                Success = true,
                Message = "Workflow performance report generated successfully",
                Data = report
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get vehicle utilization report
    /// </summary>
    [HttpGet("vehicle-utilization")]
    public async Task<IActionResult> GetVehicleUtilizationReport(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today.AddDays(1);

            var transactions = await _transactionService.GetTransactionsByDateRangeAsync(start, end);
            
            var report = new VehicleUtilizationReport
            {
                TotalVehicles = transactions.Select(t => t.VehicleId).Distinct().Count(),
                TransactionsByVehicle = transactions.GroupBy(t => t.VehicleId)
                    .ToDictionary(g => g.Key, g => g.Count()),
                AverageTransactionsPerVehicle = transactions.Any() 
                    ? (double)transactions.Count() / transactions.Select(t => t.VehicleId).Distinct().Count()
                    : 0,
                StartDate = start,
                EndDate = end
            };

            return Ok(new ApiResponse<VehicleUtilizationReport>
            {
                Success = true,
                Message = "Vehicle utilization report generated successfully",
                Data = report
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Export transactions to CSV
    /// </summary>
    [HttpGet("export/transactions")]
    public async Task<IActionResult> ExportTransactions(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today.AddDays(1);

            var transactions = await _transactionService.GetTransactionsByDateRangeAsync(start, end);
            
            var csv = GenerateTransactionsCsv(transactions);
            var fileName = $"transactions_{start:yyyyMMdd}_{end:yyyyMMdd}.csv";
            
            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    private static string GenerateTransactionsCsv(IEnumerable<TransactionDto> transactions)
    {
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("TransactionNumber,TransactionType,Status,TransactionDate,VehicleId,DriverId,OrganizationId,GrossWeight,TareWeight,NetWeight");
        
        foreach (var transaction in transactions)
        {
            csv.AppendLine($"{transaction.TransactionNumber},{transaction.TransactionType},{transaction.Status}," +
                          $"{transaction.TransactionDate:yyyy-MM-dd HH:mm:ss},{transaction.VehicleId},{transaction.DriverId}," +
                          $"{transaction.OrganizationId},{transaction.GrossWeight},{transaction.TareWeight},{transaction.NetWeight}");
        }
        
        return csv.ToString();
    }
}

public class TransactionSummaryReport
{
    public int TotalTransactions { get; set; }
    public int CompletedTransactions { get; set; }
    public int PendingTransactions { get; set; }
    public int InProgressTransactions { get; set; }
    public int CancelledTransactions { get; set; }
    public Dictionary<string, int> TransactionsByType { get; set; } = new();
    public decimal TotalWeight { get; set; }
    public decimal AverageWeight { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class FinancialReport
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalUnpaid { get; set; }
    public Dictionary<string, decimal> ChargesByType { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class WorkflowPerformanceReport
{
    public int TotalPendingSteps { get; set; }
    public Dictionary<string, int> StepsByStatus { get; set; } = new();
    public Dictionary<string, double> AverageProcessingTime { get; set; } = new();
}

public class VehicleUtilizationReport
{
    public int TotalVehicles { get; set; }
    public Dictionary<string, int> TransactionsByVehicle { get; set; } = new();
    public double AverageTransactionsPerVehicle { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}