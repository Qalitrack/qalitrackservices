using ComplianceService.Core.Entities;

namespace ComplianceService.Core.Interfaces
{
    public interface IComplianceRepository : IRepository<Compliance>
    {
        // Compliance Rules
        Task<ComplianceRule?> GetRuleByIdAsync(int id);
        Task<List<ComplianceRule>> GetActiveRulesAsync();
        Task<List<ComplianceRule>> GetRulesByTypeAsync(string ruleType);
        Task<ComplianceRule> CreateRuleAsync(ComplianceRule rule);
        Task<ComplianceRule> UpdateRuleAsync(ComplianceRule rule);
        Task DeleteRuleAsync(int id);

        // Compliance Violations
        Task<ComplianceViolation?> GetViolationByIdAsync(int id);
        Task<List<ComplianceViolation>> GetViolationsByStatusAsync(string status);
        Task<List<ComplianceViolation>> GetViolationsByOrganizationAsync(string organizationId);
        Task<List<ComplianceViolation>> GetViolationsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<ComplianceViolation> CreateViolationAsync(ComplianceViolation violation);
        Task<ComplianceViolation> UpdateViolationAsync(ComplianceViolation violation);

        // License Monitoring
        Task<List<LicenseMonitoring>> GetExpiringLicensesAsync(int daysAhead = 30);
        Task<LicenseMonitoring?> GetLicenseByDriverIdAsync(string driverId);
        Task<List<LicenseMonitoring>> GetLicensesByOrganizationAsync(string organizationId);

        // Weight Limits
        Task<WeightLimits?> GetWeightLimitsByVehicleTypeAsync(string vehicleType);
        Task<List<WeightLimits>> GetActiveWeightLimitsAsync();

        // Route Restrictions
        Task<List<RouteRestrictions>> GetRouteRestrictionsAsync(string routeId);
        Task<List<RouteRestrictions>> GetActiveRouteRestrictionsAsync();

        // Compliance Audits
        Task<ComplianceAudit> CreateAuditEntryAsync(ComplianceAudit audit);
        Task<List<ComplianceAudit>> GetAuditsByEntityAsync(string entityType, int entityId);
        
        // New methods for Compliance entity management
        Task<List<Compliance>> GetByEntityAsync(string entityType, string entityId);
        Task<List<Compliance>> GetByOrganizationAsync(string organizationId);
        Task<List<Compliance>> GetByStatusAsync(ComplianceStatus status);
        Task<List<Compliance>> GetByTypeAsync(ComplianceType complianceType);
        Task<List<Compliance>> GetByOrganizationAndDateRangeAsync(string organizationId, DateTime fromDate, DateTime toDate);
        Task<List<Compliance>> GetHighRiskItemsAsync(string organizationId);
        Task<List<Compliance>> GetItemsRequiringMonitoringAsync();
        Task<List<Compliance>> GetMonitoringScheduleAsync(string organizationId);
        Task<List<ComplianceRule>> GetRulesByIdsAsync(List<string> ruleIds);
        Task<List<ComplianceRule>> GetRulesByEntityTypeAsync(string entityType);
    }
}