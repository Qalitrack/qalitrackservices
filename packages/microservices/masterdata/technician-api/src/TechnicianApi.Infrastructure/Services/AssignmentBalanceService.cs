using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TechnicianApi.Core.DTOs.Balance;
using TechnicianApi.Core.Entities;
using TechnicianApi.Core.Interfaces;
using TechnicianApi.Infrastructure.Data;

namespace TechnicianApi.Infrastructure.Services;

public class AssignmentBalanceService : IAssignmentBalanceService
{
    private readonly TechnicianApiDbContext _context;
    private readonly IRepository<AssignmentBalanceSummary> _balanceSummaryRepository;
    private readonly IMapper _mapper;

    public AssignmentBalanceService(
        TechnicianApiDbContext context,
        IRepository<AssignmentBalanceSummary> balanceSummaryRepository,
        IMapper mapper)
    {
        _context = context;
        _balanceSummaryRepository = balanceSummaryRepository;
        _mapper = mapper;
    }

    public async Task<AssignmentBalanceSummaryResponseDto?> GetBalanceSummaryAsync(string assignmentId)
    {
        var summary = await _balanceSummaryRepository.FirstOrDefaultAsync(x => x.AssignmentId == assignmentId);

        if (summary == null)
        {
            return null;
        }

        return _mapper.Map<AssignmentBalanceSummaryResponseDto>(summary);
    }

    public async Task<AssignmentBalanceSummaryResponseDto> CalculateAndUpdateBalanceAsync(string assignmentId)
    {
        var assignment = await _context.Assignments
            .Include(a => a.PettyCashAdvanceForms)
            .Include(a => a.AdvanceReturnForms)
                .ThenInclude(f => f.LineItems)
            .Include(a => a.PerDiemReturnForms)
            .Include(a => a.Requisitions)
            .Include(a => a.Claims)
            .Include(a => a.Refunds)
            .Include(a => a.BalanceSummary)
            .FirstOrDefaultAsync(a => a.Id == assignmentId);

        if (assignment == null)
        {
            throw new InvalidOperationException($"Assignment with ID {assignmentId} not found");
        }

        // Calculate Money OUT (Company → Technician)
        var totalPettyCashAdvances = assignment.PettyCashAdvanceForms
            .Where(f => f.Status == PettyCashStatus.Disbursed)
            .Sum(f => f.Sum);

        var totalCashAdvanceRequisitions = assignment.Requisitions
            .Where(r => r.Type == RequisitionType.CashAdvance && r.Status == RequisitionStatus.Paid)
            .Sum(r => r.Amount);

        var totalApprovedClaims = assignment.Claims
            .Where(c => c.Status == ClaimStatus.Disbursed)
            .Sum(c => c.Amount);

        var totalAdvancesGiven = totalPettyCashAdvances + totalCashAdvanceRequisitions + totalApprovedClaims;

        // Calculate Money ACCOUNTED (Technician → Company)
        var totalReturns = assignment.AdvanceReturnForms
            .Where(f => f.Status == AdvanceReturnStatus.Approved)
            .Sum(f => f.TotalAmount);

        var totalExpensesClaimed = assignment.PerDiemReturnForms
            .Where(f => f.Status == PerDiemReturnStatus.Approved)
            .Sum(f => f.TotalAmount);

        var totalMaterialsRequisitioned = assignment.Requisitions
            .Where(r => r.Type == RequisitionType.MaterialRequisition && r.Status == RequisitionStatus.Paid)
            .Sum(r => r.Amount);

        var totalRefunds = assignment.Refunds
            .Where(r => r.Status == RefundStatus.CfoReceived)
            .Sum(r => r.Amount);

        var totalMoneyAccountedFor = totalReturns + totalExpensesClaimed + totalMaterialsRequisitioned + totalRefunds;

        // Calculate Net Balance
        var netBalance = totalAdvancesGiven - totalMoneyAccountedFor;

        // Determine Balance Status
        var balanceStatus = netBalance switch
        {
            > 0 => BalanceStatus.TechnicianOwes,
            < 0 => BalanceStatus.CompanyOwes,
            _ => BalanceStatus.Balanced
        };

        // Determine Recommended Action
        var recommendedAction = netBalance switch
        {
            > 0 => RecommendedAction.CreateRefund,
            < 0 => RecommendedAction.CreateClaim,
            _ => RecommendedAction.None
        };

        var recommendedAmount = Math.Abs(netBalance);
        var recommendationMessage = netBalance switch
        {
            > 0 => $"Technician owes KSH {recommendedAmount:N2}. Please create a Refund form.",
            < 0 => $"Company owes technician KSH {recommendedAmount:N2}. Technician can create a Claim.",
            _ => "Balance is settled. No action needed."
        };

        // Get technician ID from assignment
        var technicianId = assignment.Technicians.FirstOrDefault()?.Id ?? assignment.AssignmentTechnicians.FirstOrDefault()?.TechnicianId ?? string.Empty;

        // Update or Create Balance Summary
        var summary = assignment.BalanceSummary;
        if (summary == null)
        {
            summary = new AssignmentBalanceSummary
            {
                Id = Guid.NewGuid().ToString(),
                AssignmentId = assignmentId,
                TechnicianId = technicianId,
                CreatedAt = DateTime.UtcNow
            };
            _context.AssignmentBalanceSummaries.Add(summary);
        }

        summary.TotalAdvancesGiven = totalAdvancesGiven;
        summary.TotalReturns = totalReturns;
        summary.TotalExpensesClaimed = totalExpensesClaimed;
        summary.TotalMaterialsRequisitioned = totalMaterialsRequisitioned;
        summary.TotalRefunds = totalRefunds;
        summary.TotalMoneyOut = totalAdvancesGiven;
        summary.TotalMoneyAccountedFor = totalMoneyAccountedFor;
        summary.NetBalance = netBalance;
        summary.BalanceStatus = balanceStatus;
        summary.RecommendedAction = recommendedAction;
        summary.RecommendedAmount = recommendedAmount;
        summary.RecommendationMessage = recommendationMessage;
        summary.LastCalculatedAt = DateTime.UtcNow;
        summary.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return _mapper.Map<AssignmentBalanceSummaryResponseDto>(summary);
    }

    public async Task RecalculateOnFormChangeAsync(string assignmentId)
    {
        await CalculateAndUpdateBalanceAsync(assignmentId);
    }
}
