using ComplianceService.Core.Entities;
using ComplianceService.Core.Interfaces;
using ComplianceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ComplianceService.Infrastructure.Repositories
{
    public class ComplianceRepository : IComplianceRepository
    {
        private readonly ComplianceDbContext _context;
        private readonly ILogger<ComplianceRepository> _logger;

        public ComplianceRepository(ComplianceDbContext context, ILogger<ComplianceRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Compliance Rules
        public async Task<ComplianceRule?> GetRuleByIdAsync(string id)
        {
            return await _context.ComplianceRules
                .Include(r => r.Violations)
                .Include(r => r.ComplianceChecks)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<ComplianceRule>> GetActiveRulesAsync()
        {
            return await _context.ComplianceRules
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<List<ComplianceRule>> GetRulesByTypeAsync(string ruleType)
        {
            return await _context.ComplianceRules
                .Where(r => r.RuleType == ruleType && r.IsActive)
                .OrderBy(r => r.Category)
                .ThenBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<ComplianceRule> CreateRuleAsync(ComplianceRule rule)
        {
            rule.CreatedAt = DateTime.UtcNow;
            rule.UpdatedAt = DateTime.UtcNow;

            _context.ComplianceRules.Add(rule);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created compliance rule {RuleId}: {RuleName}", rule.Id, rule.Name);
            return rule;
        }

        public async Task<ComplianceRule> UpdateRuleAsync(ComplianceRule rule)
        {
            rule.UpdatedAt = DateTime.UtcNow;

            _context.ComplianceRules.Update(rule);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated compliance rule {RuleId}: {RuleName}", rule.Id, rule.Name);
            return rule;
        }

        public async Task DeleteRuleAsync(int id)
        {
            var rule = await _context.ComplianceRules.FindAsync(id);
            if (rule != null)
            {
                // Soft delete by setting IsActive to false
                rule.IsActive = false;
                rule.UpdatedAt = DateTime.UtcNow;
                
                _context.ComplianceRules.Update(rule);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deactivated compliance rule {RuleId}: {RuleName}", rule.Id, rule.Name);
            }
        }

        // Compliance Violations
        public async Task<ComplianceViolation?> GetViolationByIdAsync(int id)
        {
            return await _context.ComplianceViolations
                .Include(v => v.ComplianceRule)
                .Include(v => v.AuditEntries)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<List<ComplianceViolation>> GetViolationsByStatusAsync(string status)
        {
            return await _context.ComplianceViolations
                .Include(v => v.ComplianceRule)
                .Where(v => v.Status == status)
                .OrderByDescending(v => v.DetectedAt)
                .ToListAsync();
        }

        public async Task<List<ComplianceViolation>> GetViolationsByOrganizationAsync(string organizationId)
        {
            return await _context.ComplianceViolations
                .Include(v => v.ComplianceRule)
                .Where(v => v.OrganizationId == organizationId)
                .OrderByDescending(v => v.DetectedAt)
                .ToListAsync();
        }

        public async Task<List<ComplianceViolation>> GetViolationsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.ComplianceViolations
                .Include(v => v.ComplianceRule)
                .Where(v => v.DetectedAt >= startDate && v.DetectedAt <= endDate)
                .OrderByDescending(v => v.DetectedAt)
                .ToListAsync();
        }

        public async Task<ComplianceViolation> CreateViolationAsync(ComplianceViolation violation)
        {
            violation.CreatedAt = DateTime.UtcNow;
            violation.UpdatedAt = DateTime.UtcNow;

            _context.ComplianceViolations.Add(violation);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created compliance violation {ViolationId}: {ViolationType} for {EntityId}", 
                violation.Id, violation.ViolationType, violation.VehicleId ?? violation.DriverId);

            return violation;
        }

        public async Task<ComplianceViolation> UpdateViolationAsync(ComplianceViolation violation)
        {
            violation.UpdatedAt = DateTime.UtcNow;

            _context.ComplianceViolations.Update(violation);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated compliance violation {ViolationId}: status changed to {Status}", 
                violation.Id, violation.Status);

            return violation;
        }

        // License Monitoring
        public async Task<List<LicenseMonitoring>> GetExpiringLicensesAsync(int daysAhead = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);
            
            return await _context.LicenseMonitoring
                .Where(l => l.IsMonitored && 
                           l.ExpiryDate <= cutoffDate && 
                           l.ExpiryDate >= DateTime.UtcNow &&
                           l.Status == "ACTIVE")
                .OrderBy(l => l.ExpiryDate)
                .ToListAsync();
        }

        public async Task<LicenseMonitoring?> GetLicenseByDriverIdAsync(string driverId)
        {
            return await _context.LicenseMonitoring
                .Where(l => l.DriverId == driverId && l.IsMonitored)
                .OrderByDescending(l => l.ExpiryDate)
                .FirstOrDefaultAsync();
        }

        public async Task<List<LicenseMonitoring>> GetLicensesByOrganizationAsync(string organizationId)
        {
            return await _context.LicenseMonitoring
                .Where(l => l.OrganizationId == organizationId && l.IsMonitored)
                .OrderBy(l => l.ExpiryDate)
                .ToListAsync();
        }

        // Weight Limits
        public async Task<WeightLimits?> GetWeightLimitsByVehicleTypeAsync(string vehicleType)
        {
            return await _context.WeightLimits
                .Where(w => w.VehicleType == vehicleType && w.IsActive)
                .OrderByDescending(w => w.EffectiveDate)
                .FirstOrDefaultAsync();
        }

        public async Task<List<WeightLimits>> GetActiveWeightLimitsAsync()
        {
            return await _context.WeightLimits
                .Where(w => w.IsActive && 
                           (w.ExpiryDate == null || w.ExpiryDate > DateTime.UtcNow))
                .OrderBy(w => w.VehicleType)
                .ThenBy(w => w.VehicleClass)
                .ToListAsync();
        }

        // Route Restrictions
        public async Task<List<RouteRestrictions>> GetRouteRestrictionsAsync(string routeId)
        {
            return await _context.RouteRestrictions
                .Where(r => r.RouteId == routeId && r.IsActive &&
                           (r.ExpiryDate == null || r.ExpiryDate > DateTime.UtcNow))
                .OrderBy(r => r.RestrictionType)
                .ToListAsync();
        }

        public async Task<List<RouteRestrictions>> GetActiveRouteRestrictionsAsync()
        {
            return await _context.RouteRestrictions
                .Where(r => r.IsActive && 
                           (r.ExpiryDate == null || r.ExpiryDate > DateTime.UtcNow))
                .OrderBy(r => r.RouteId)
                .ThenBy(r => r.RestrictionType)
                .ToListAsync();
        }

        // Compliance Audits
        public async Task<ComplianceAudit> CreateAuditEntryAsync(ComplianceAudit audit)
        {
            audit.Timestamp = DateTime.UtcNow;

            _context.ComplianceAudits.Add(audit);
            await _context.SaveChangesAsync();

            _logger.LogDebug("Created audit entry {AuditId}: {Action} on {EntityType} {EntityId}", 
                audit.Id, audit.Action, audit.EntityType, audit.EntityId);

            return audit;
        }

        public async Task<List<ComplianceAudit>> GetAuditsByEntityAsync(string entityType, int entityId)
        {
            return await _context.ComplianceAudits
                .Where(a => a.EntityType == entityType && a.EntityId == entityId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        // Additional utility methods
        public async Task<List<ComplianceViolation>> GetViolationsByTransactionAsync(string transactionId)
        {
            return await _context.ComplianceViolations
                .Include(v => v.ComplianceRule)
                .Where(v => v.TransactionId == transactionId)
                .OrderByDescending(v => v.DetectedAt)
                .ToListAsync();
        }

        public async Task<List<ComplianceViolation>> GetViolationsByVehicleAsync(string vehicleId, DateTime? fromDate = null)
        {
            var query = _context.ComplianceViolations
                .Include(v => v.ComplianceRule)
                .Where(v => v.VehicleId == vehicleId);

            if (fromDate.HasValue)
            {
                query = query.Where(v => v.DetectedAt >= fromDate.Value);
            }

            return await query
                .OrderByDescending(v => v.DetectedAt)
                .ToListAsync();
        }

        public async Task<List<ComplianceViolation>> GetViolationsByDriverAsync(string driverId, DateTime? fromDate = null)
        {
            var query = _context.ComplianceViolations
                .Include(v => v.ComplianceRule)
                .Where(v => v.DriverId == driverId);

            if (fromDate.HasValue)
            {
                query = query.Where(v => v.DetectedAt >= fromDate.Value);
            }

            return await query
                .OrderByDescending(v => v.DetectedAt)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetViolationStatsByTypeAsync(string organizationId, DateTime? fromDate = null)
        {
            var query = _context.ComplianceViolations
                .Where(v => v.OrganizationId == organizationId);

            if (fromDate.HasValue)
            {
                query = query.Where(v => v.DetectedAt >= fromDate.Value);
            }

            return await query
                .GroupBy(v => v.ViolationType)
                .Select(g => new { ViolationType = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ViolationType, x => x.Count);
        }

        public async Task<decimal> GetTotalPenaltiesAsync(string organizationId, DateTime? fromDate = null)
        {
            var query = _context.ComplianceViolations
                .Where(v => v.OrganizationId == organizationId && v.PenaltyAmount.HasValue);

            if (fromDate.HasValue)
            {
                query = query.Where(v => v.DetectedAt >= fromDate.Value);
            }

            return await query.SumAsync(v => v.PenaltyAmount ?? 0);
        }

        public async Task<bool> BulkResolveViolationsAsync(List<int> violationIds, string resolvedBy, string resolutionNotes)
        {
            try
            {
                var violations = await _context.ComplianceViolations
                    .Where(v => violationIds.Contains(v.Id))
                    .ToListAsync();

                foreach (var violation in violations)
                {
                    violation.Status = "RESOLVED";
                    violation.ResolvedAt = DateTime.UtcNow;
                    violation.ResolvedBy = resolvedBy;
                    violation.ResolutionNotes = resolutionNotes;
                    violation.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Bulk resolved {Count} violations by {ResolvedBy}", violations.Count, resolvedBy);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk resolving violations");
                return false;
            }
        }

        public async Task<bool> AcknowledgeViolationAsync(int violationId, string acknowledgedBy)
        {
            try
            {
                var violation = await _context.ComplianceViolations.FindAsync(violationId);
                if (violation != null)
                {
                    violation.IsAcknowledged = true;
                    violation.AcknowledgedAt = DateTime.UtcNow;
                    violation.AcknowledgedBy = acknowledgedBy;
                    violation.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Acknowledged violation {ViolationId} by {AcknowledgedBy}", violationId, acknowledgedBy);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acknowledging violation {ViolationId}", violationId);
                return false;
            }
        }

        // Missing IRepository<Compliance> base methods - these need to be implemented
        public async Task<List<Compliance>> GetByEntityAsync(string entityType, string entityId)
        {
            return await _context.Compliances
                .Where(c => c.EntityType == entityType && c.EntityId == entityId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Compliance>> GetByOrganizationAsync(string organizationId)
        {
            return await _context.Compliances
                .Where(c => c.OrganizationId == organizationId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Compliance>> GetByStatusAsync(ComplianceStatus status)
        {
            return await _context.Compliances
                .Where(c => c.Status == status)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Compliance>> GetByTypeAsync(ComplianceType complianceType)
        {
            return await _context.Compliances
                .Where(c => c.ComplianceType == complianceType)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Compliance>> GetByOrganizationAndDateRangeAsync(string organizationId, DateTime fromDate, DateTime toDate)
        {
            return await _context.Compliances
                .Where(c => c.OrganizationId == organizationId && 
                           c.CreatedAt >= fromDate && 
                           c.CreatedAt <= toDate)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Compliance>> GetHighRiskItemsAsync(string organizationId)
        {
            return await _context.Compliances
                .Where(c => c.OrganizationId == organizationId && 
                           (c.RiskLevel == RiskLevel.High || c.RiskLevel == RiskLevel.Critical || c.RiskLevel == RiskLevel.Extreme))
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Compliance>> GetItemsRequiringMonitoringAsync()
        {
            return await _context.Compliances
                .Where(c => c.RequiresContinuousMonitoring && c.Status != ComplianceStatus.Compliant)
                .OrderBy(c => c.NextCheckDate)
                .ToListAsync();
        }

        public async Task<List<Compliance>> GetMonitoringScheduleAsync(string organizationId)
        {
            return await _context.Compliances
                .Where(c => c.OrganizationId == organizationId && 
                           c.RequiresContinuousMonitoring &&
                           c.NextCheckDate <= DateTime.UtcNow.AddDays(30))
                .OrderBy(c => c.NextCheckDate)
                .ToListAsync();
        }

        public async Task<List<ComplianceRule>> GetRulesByIdsAsync(List<string> ruleIds)
        {
            return await _context.ComplianceRules
                .Where(r => ruleIds.Contains(r.Id))
                .ToListAsync();
        }

        public async Task<List<ComplianceRule>> GetRulesByEntityTypeAsync(string entityType)
        {
            return await _context.ComplianceRules
                .Where(r => r.RuleType == entityType && r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        // Base IRepository<Compliance> interface methods
        public async Task<Compliance?> GetByIdAsync(string id)
        {
            return await _context.Compliances
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Compliance>> GetAllAsync()
        {
            return await _context.Compliances
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Compliance>> FindAsync(System.Linq.Expressions.Expression<Func<Compliance, bool>> predicate)
        {
            return await _context.Compliances
                .Where(predicate)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Compliance?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<Compliance, bool>> predicate)
        {
            return await _context.Compliances
                .FirstOrDefaultAsync(predicate);
        }

        public async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<Compliance, bool>> predicate)
        {
            return await _context.Compliances
                .AnyAsync(predicate);
        }

        public async Task<int> CountAsync(System.Linq.Expressions.Expression<Func<Compliance, bool>>? predicate = null)
        {
            if (predicate == null)
                return await _context.Compliances.CountAsync();
                
            return await _context.Compliances
                .CountAsync(predicate);
        }

        public async Task<Compliance> AddAsync(Compliance entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            _context.Compliances.Add(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created compliance record {ComplianceId} for {EntityType} {EntityId}", 
                entity.Id, entity.EntityType, entity.EntityId);

            return entity;
        }

        public async Task<IEnumerable<Compliance>> AddRangeAsync(IEnumerable<Compliance> entities)
        {
            var entitiesList = entities.ToList();
            var now = DateTime.UtcNow;

            foreach (var entity in entitiesList)
            {
                entity.CreatedAt = now;
                entity.UpdatedAt = now;
            }

            _context.Compliances.AddRange(entitiesList);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created {Count} compliance records", entitiesList.Count);

            return entitiesList;
        }

        public async Task<Compliance> UpdateAsync(Compliance entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;

            _context.Compliances.Update(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated compliance record {ComplianceId}", entity.Id);

            return entity;
        }

        public async Task<Compliance> DeleteAsync(Compliance entity)
        {
            _context.Compliances.Remove(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted compliance record {ComplianceId}", entity.Id);

            return entity;
        }

        public async Task<bool> DeleteByIdAsync(string id)
        {
            var entity = await _context.Compliances.FindAsync(id);
            if (entity == null)
                return false;

            _context.Compliances.Remove(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted compliance record {ComplianceId}", id);

            return true;
        }

        public async Task<IEnumerable<Compliance>> GetPagedAsync(int pageNumber, int pageSize, System.Linq.Expressions.Expression<Func<Compliance, bool>>? predicate = null)
        {
            var query = _context.Compliances.AsQueryable();

            if (predicate != null)
                query = query.Where(predicate);

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}