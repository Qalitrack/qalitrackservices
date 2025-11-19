using TechnicianApi.Core.DTOs.Balance;

namespace TechnicianApi.Core.Interfaces;

public interface IAssignmentBalanceService
{
    Task<AssignmentBalanceSummaryResponseDto?> GetBalanceSummaryAsync(string assignmentId);
    Task<AssignmentBalanceSummaryResponseDto> CalculateAndUpdateBalanceAsync(string assignmentId);
    Task RecalculateOnFormChangeAsync(string assignmentId);
}
