using ComplianceService.Core.DTOs;

namespace ComplianceService.Core.Interfaces
{
    public interface IComplianceRulesEngine
    {
        Task<ComplianceResultDto> EvaluateWeightComplianceAsync(WeightMeasurementDto measurement);
        Task<ComplianceResultDto> EvaluateLicenseComplianceAsync(string driverId);
        Task<ComplianceResultDto> EvaluateRouteComplianceAsync(string routeId, string vehicleId);
        Task<ComplianceResultDto> EvaluateProductComplianceAsync(string productId, string vehicleId);
        Task<List<ComplianceViolationDto>> DetectViolationsAsync(string transactionId);
        Task<ComplianceResultDto> EvaluateAllComplianceAsync(TransactionComplianceDto transaction);
    }
}