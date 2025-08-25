using ComplianceService.Core.DTOs;
using ComplianceService.Core.Entities;

namespace ComplianceService.Core.Interfaces
{
    public interface IViolationDetectionService
    {
        Task<List<ComplianceViolation>> DetectWeightViolationsAsync(WeightMeasurementDto measurement);
        Task<List<ComplianceViolation>> DetectLicenseViolationsAsync(string driverId);
        Task<List<ComplianceViolation>> DetectRouteViolationsAsync(string routeId, string vehicleId, DateTime timestamp);
        Task<List<ComplianceViolation>> DetectProductViolationsAsync(string productId, string vehicleId);
        Task<List<ComplianceViolation>> DetectAllViolationsAsync(TransactionComplianceDto transaction);
        Task<List<ComplianceViolation>> DetectViolationsAsync(ComplianceResultDto result);
    }
}