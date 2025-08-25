using ComplianceService.Core.DTOs;
using ComplianceService.Core.Interfaces;
using ComplianceService.Core.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ComplianceService.Core.RulesEngine
{
    public class ComplianceRulesEngine : IComplianceRulesEngine
    {
        private readonly IComplianceRepository _repository;
        private readonly IViolationDetectionService _violationDetection;
        private readonly ILogger<ComplianceRulesEngine> _logger;

        public ComplianceRulesEngine(
            IComplianceRepository repository,
            IViolationDetectionService violationDetection,
            ILogger<ComplianceRulesEngine> logger)
        {
            _repository = repository;
            _violationDetection = violationDetection;
            _logger = logger;
        }

        public async Task<ComplianceResultDto> EvaluateWeightComplianceAsync(WeightMeasurementDto measurement)
        {
            _logger.LogInformation("Evaluating weight compliance for vehicle {VehicleId}", measurement.VehicleId);

            var result = new ComplianceResultDto
            {
                CheckedAt = DateTime.UtcNow,
                CheckedBy = "WEIGHT_RULES_ENGINE"
            };

            try
            {
                // Get weight-related compliance rules
                var weightRules = await _repository.GetRulesByTypeAsync("WEIGHT");
                var activeRules = weightRules.Where(r => r.IsActive).ToList();

                var violations = new List<ComplianceViolation>();
                var warnings = new List<ComplianceWarningDto>();

                foreach (var rule in activeRules)
                {
                    var ruleViolations = await EvaluateWeightRule(rule, measurement);
                    violations.AddRange(ruleViolations);
                }

                // Check for warnings (near-limit conditions)
                await CheckWeightWarnings(measurement, warnings);

                // Calculate total penalty
                var totalPenalty = violations.Sum(v => v.PenaltyAmount ?? 0);

                result.IsCompliant = violations.Count == 0;
                result.Status = violations.Count == 0 ? "COMPLIANT" : "NON_COMPLIANT";
                result.Severity = DetermineSeverity(violations);
                result.Violations = violations.Select(MapToViolationDto).ToList();
                result.Warnings = warnings;
                result.TotalPenalty = totalPenalty;
                result.Summary = GenerateWeightComplianceSummary(measurement, violations, warnings);

                _logger.LogInformation("Weight compliance evaluation completed. Compliant: {IsCompliant}, Violations: {ViolationCount}",
                    result.IsCompliant, violations.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating weight compliance for vehicle {VehicleId}", measurement.VehicleId);
                result.Status = "ERROR";
                result.Summary = $"Error evaluating weight compliance: {ex.Message}";
            }

            return result;
        }

        public async Task<ComplianceResultDto> EvaluateLicenseComplianceAsync(string driverId)
        {
            _logger.LogInformation("Evaluating license compliance for driver {DriverId}", driverId);

            var result = new ComplianceResultDto
            {
                CheckedAt = DateTime.UtcNow,
                CheckedBy = "LICENSE_RULES_ENGINE"
            };

            try
            {
                var violations = await _violationDetection.DetectLicenseViolationsAsync(driverId);
                var warnings = new List<ComplianceWarningDto>();

                // Check for license expiry warnings
                var license = await _repository.GetLicenseByDriverIdAsync(driverId);
                if (license != null)
                {
                    if (license.IsExpiringSoon)
                    {
                        warnings.Add(new ComplianceWarningDto
                        {
                            WarningType = "LICENSE_EXPIRING",
                            Message = $"Driver license expires in {license.DaysToExpiry} days",
                            Severity = license.DaysToExpiry <= 7 ? "HIGH" : "MEDIUM",
                            Category = "LICENSE",
                            CurrentValue = license.DaysToExpiry,
                            ThresholdValue = license.ExpiryWarningDays,
                            Unit = "DAYS",
                            Recommendation = "Renew license before expiry to avoid violations"
                        });
                    }
                }

                result.IsCompliant = violations.Count == 0;
                result.Status = violations.Count == 0 ? "COMPLIANT" : "NON_COMPLIANT";
                result.Severity = DetermineSeverity(violations);
                result.Violations = violations.Select(MapToViolationDto).ToList();
                result.Warnings = warnings;
                result.Summary = GenerateLicenseComplianceSummary(driverId, violations, warnings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating license compliance for driver {DriverId}", driverId);
                result.Status = "ERROR";
                result.Summary = $"Error evaluating license compliance: {ex.Message}";
            }

            return result;
        }

        public async Task<ComplianceResultDto> EvaluateRouteComplianceAsync(string routeId, string vehicleId)
        {
            _logger.LogInformation("Evaluating route compliance for route {RouteId}, vehicle {VehicleId}", routeId, vehicleId);

            var result = new ComplianceResultDto
            {
                CheckedAt = DateTime.UtcNow,
                CheckedBy = "ROUTE_RULES_ENGINE"
            };

            try
            {
                var violations = await _violationDetection.DetectRouteViolationsAsync(routeId, vehicleId, DateTime.UtcNow);
                var warnings = new List<ComplianceWarningDto>();

                // Check for route restriction warnings
                var restrictions = await _repository.GetRouteRestrictionsAsync(routeId);
                foreach (var restriction in restrictions.Where(r => r.IsActive))
                {
                    if (restriction.IsCurrentlyRestricted)
                    {
                        warnings.Add(new ComplianceWarningDto
                        {
                            WarningType = "ROUTE_TIME_RESTRICTION",
                            Message = $"Route {routeId} has time restrictions currently active",
                            Severity = "MEDIUM",
                            Category = "ROUTE",
                            Recommendation = "Consider using alternative route or wait for restriction period to end"
                        });
                    }
                }

                result.IsCompliant = violations.Count == 0;
                result.Status = violations.Count == 0 ? "COMPLIANT" : "NON_COMPLIANT";
                result.Severity = DetermineSeverity(violations);
                result.Violations = violations.Select(MapToViolationDto).ToList();
                result.Warnings = warnings;
                result.Summary = GenerateRouteComplianceSummary(routeId, vehicleId, violations, warnings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating route compliance for route {RouteId}, vehicle {VehicleId}", routeId, vehicleId);
                result.Status = "ERROR";
                result.Summary = $"Error evaluating route compliance: {ex.Message}";
            }

            return result;
        }

        public async Task<ComplianceResultDto> EvaluateProductComplianceAsync(string productId, string vehicleId)
        {
            _logger.LogInformation("Evaluating product compliance for product {ProductId}, vehicle {VehicleId}", productId, vehicleId);

            var result = new ComplianceResultDto
            {
                CheckedAt = DateTime.UtcNow,
                CheckedBy = "PRODUCT_RULES_ENGINE"
            };

            try
            {
                var violations = await _violationDetection.DetectProductViolationsAsync(productId, vehicleId);

                result.IsCompliant = violations.Count == 0;
                result.Status = violations.Count == 0 ? "COMPLIANT" : "NON_COMPLIANT";
                result.Severity = DetermineSeverity(violations);
                result.Violations = violations.Select(MapToViolationDto).ToList();
                result.Summary = GenerateProductComplianceSummary(productId, vehicleId, violations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating product compliance for product {ProductId}, vehicle {VehicleId}", productId, vehicleId);
                result.Status = "ERROR";
                result.Summary = $"Error evaluating product compliance: {ex.Message}";
            }

            return result;
        }

        public async Task<List<ComplianceViolationDto>> DetectViolationsAsync(string transactionId)
        {
            _logger.LogInformation("Detecting violations for transaction {TransactionId}", transactionId);

            try
            {
                // This would typically fetch transaction details from the transaction service
                // For now, we'll create a mock transaction compliance check
                var violations = new List<ComplianceViolation>();

                // In a real implementation, you would:
                // 1. Fetch transaction details from transaction service
                // 2. Get associated weight measurements, driver info, route info, etc.
                // 3. Run all compliance checks
                // 4. Return consolidated violations

                return violations.Select(MapToViolationDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting violations for transaction {TransactionId}", transactionId);
                return new List<ComplianceViolationDto>();
            }
        }

        public async Task<ComplianceResultDto> EvaluateAllComplianceAsync(TransactionComplianceDto transaction)
        {
            _logger.LogInformation("Evaluating all compliance for transaction {TransactionId}", transaction.TransactionId);

            var result = new ComplianceResultDto
            {
                CheckedAt = DateTime.UtcNow,
                CheckedBy = "COMPREHENSIVE_RULES_ENGINE"
            };

            try
            {
                var allViolations = new List<ComplianceViolation>();
                var allWarnings = new List<ComplianceWarningDto>();

                // Weight compliance
                if (transaction.WeightMeasurement != null)
                {
                    var weightResult = await EvaluateWeightComplianceAsync(transaction.WeightMeasurement);
                    allViolations.AddRange(await _violationDetection.DetectWeightViolationsAsync(transaction.WeightMeasurement));
                    allWarnings.AddRange(weightResult.Warnings);
                }

                // License compliance
                if (!string.IsNullOrEmpty(transaction.DriverId))
                {
                    var licenseResult = await EvaluateLicenseComplianceAsync(transaction.DriverId);
                    allViolations.AddRange(await _violationDetection.DetectLicenseViolationsAsync(transaction.DriverId));
                    allWarnings.AddRange(licenseResult.Warnings);
                }

                // Route compliance
                if (!string.IsNullOrEmpty(transaction.RouteId) && !string.IsNullOrEmpty(transaction.VehicleId))
                {
                    var routeResult = await EvaluateRouteComplianceAsync(transaction.RouteId, transaction.VehicleId);
                    allViolations.AddRange(await _violationDetection.DetectRouteViolationsAsync(transaction.RouteId, transaction.VehicleId, transaction.TransactionTime));
                    allWarnings.AddRange(routeResult.Warnings);
                }

                // Product compliance
                if (!string.IsNullOrEmpty(transaction.ProductId) && !string.IsNullOrEmpty(transaction.VehicleId))
                {
                    var productResult = await EvaluateProductComplianceAsync(transaction.ProductId, transaction.VehicleId);
                    allViolations.AddRange(await _violationDetection.DetectProductViolationsAsync(transaction.ProductId, transaction.VehicleId));
                }

                var totalPenalty = allViolations.Sum(v => v.PenaltyAmount ?? 0);

                result.IsCompliant = allViolations.Count == 0;
                result.Status = allViolations.Count == 0 ? "COMPLIANT" : "NON_COMPLIANT";
                result.Severity = DetermineSeverity(allViolations);
                result.Violations = allViolations.Select(MapToViolationDto).ToList();
                result.Warnings = allWarnings;
                result.TotalPenalty = totalPenalty;
                result.Summary = GenerateComprehensiveComplianceSummary(transaction, allViolations, allWarnings);

                _logger.LogInformation("Comprehensive compliance evaluation completed for transaction {TransactionId}. Compliant: {IsCompliant}, Violations: {ViolationCount}",
                    transaction.TransactionId, result.IsCompliant, allViolations.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating comprehensive compliance for transaction {TransactionId}", transaction.TransactionId);
                result.Status = "ERROR";
                result.Summary = $"Error evaluating compliance: {ex.Message}";
            }

            return result;
        }

        private async Task<List<ComplianceViolation>> EvaluateWeightRule(ComplianceRule rule, WeightMeasurementDto measurement)
        {
            var violations = new List<ComplianceViolation>();

            try
            {
                var config = JsonSerializer.Deserialize<Dictionary<string, object>>(rule.ConfigurationJson);
                
                switch (rule.Category)
                {
                    case "GROSS_WEIGHT":
                        if (rule.MaxValue.HasValue && measurement.GrossWeight > rule.MaxValue.Value)
                        {
                            violations.Add(CreateWeightViolation(rule, measurement, "GROSS_WEIGHT_EXCEEDED",
                                measurement.GrossWeight, rule.MaxValue.Value));
                        }
                        break;

                    case "AXLE_WEIGHT":
                        foreach (var axle in measurement.AxleWeights)
                        {
                            if (rule.MaxValue.HasValue && axle.Weight > rule.MaxValue.Value)
                            {
                                violations.Add(CreateWeightViolation(rule, measurement, "AXLE_WEIGHT_EXCEEDED",
                                    axle.Weight, rule.MaxValue.Value));
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating weight rule {RuleId} for measurement {MeasurementId}", rule.Id, measurement.Id);
            }

            return violations;
        }

        private ComplianceViolation CreateWeightViolation(ComplianceRule rule, WeightMeasurementDto measurement, 
            string violationType, decimal actualValue, decimal limitValue)
        {
            var excessValue = actualValue - limitValue;
            var penaltyAmount = excessValue * (rule.PenaltyAmount ?? 0);

            return new ComplianceViolation
            {
                ComplianceRuleId = rule.Id,
                ViolationType = violationType,
                TransactionId = measurement.TransactionId,
                VehicleId = measurement.VehicleId,
                DriverId = measurement.DriverId,
                WeighbridgeId = measurement.WeighbridgeId,
                OrganizationId = measurement.OrganizationId,
                Status = "ACTIVE",
                Severity = rule.Severity,
                ActualValue = actualValue,
                LimitValue = limitValue,
                ExcessValue = excessValue,
                Unit = measurement.Unit,
                PenaltyAmount = penaltyAmount,
                PenaltyCurrency = rule.PenaltyCurrency,
                Description = $"{violationType}: {actualValue} {measurement.Unit} exceeds limit of {limitValue} {measurement.Unit}",
                Details = JsonSerializer.Serialize(new
                {
                    MeasurementDetails = measurement,
                    RuleConfiguration = rule.ConfigurationJson
                }),
                DetectedAt = DateTime.UtcNow
            };
        }

        private async Task CheckWeightWarnings(WeightMeasurementDto measurement, List<ComplianceWarningDto> warnings)
        {
            // Check if weight is approaching limits (e.g., within 90% of limit)
            var weightLimits = await _repository.GetWeightLimitsByVehicleTypeAsync(measurement.VehicleType);
            if (weightLimits != null)
            {
                var warningThreshold = weightLimits.MaxGrossWeight * 0.9m;
                if (measurement.GrossWeight > warningThreshold && measurement.GrossWeight <= weightLimits.MaxGrossWeight)
                {
                    warnings.Add(new ComplianceWarningDto
                    {
                        WarningType = "APPROACHING_WEIGHT_LIMIT",
                        Message = "Vehicle weight is approaching maximum limit",
                        Severity = "MEDIUM",
                        Category = "WEIGHT",
                        CurrentValue = measurement.GrossWeight,
                        ThresholdValue = weightLimits.MaxGrossWeight,
                        Unit = "KG",
                        Recommendation = "Consider reducing load to avoid violations"
                    });
                }
            }
        }

        private ComplianceViolationDto MapToViolationDto(ComplianceViolation violation)
        {
            return new ComplianceViolationDto
            {
                Id = violation.Id,
                ComplianceRuleId = violation.ComplianceRuleId,
                ViolationType = violation.ViolationType,
                TransactionId = violation.TransactionId,
                VehicleId = violation.VehicleId,
                DriverId = violation.DriverId,
                WeighbridgeId = violation.WeighbridgeId,
                OrganizationId = violation.OrganizationId,
                Status = violation.Status,
                Severity = violation.Severity,
                ActualValue = violation.ActualValue,
                LimitValue = violation.LimitValue,
                ExcessValue = violation.ExcessValue,
                Unit = violation.Unit,
                PenaltyAmount = violation.PenaltyAmount,
                PenaltyCurrency = violation.PenaltyCurrency,
                Description = violation.Description,
                Details = violation.Details,
                DetectedAt = violation.DetectedAt,
                ResolvedAt = violation.ResolvedAt,
                ResolvedBy = violation.ResolvedBy,
                ResolutionNotes = violation.ResolutionNotes,
                IsAcknowledged = violation.IsAcknowledged,
                AcknowledgedAt = violation.AcknowledgedAt,
                AcknowledgedBy = violation.AcknowledgedBy,
                RuleName = violation.ComplianceRule?.Name ?? "",
                RuleCategory = violation.ComplianceRule?.Category ?? ""
            };
        }

        private string DetermineSeverity(List<ComplianceViolation> violations)
        {
            if (violations.Any(v => v.Severity == "CRITICAL")) return "CRITICAL";
            if (violations.Any(v => v.Severity == "HIGH")) return "HIGH";
            if (violations.Any(v => v.Severity == "MEDIUM")) return "MEDIUM";
            return "LOW";
        }

        private string GenerateWeightComplianceSummary(WeightMeasurementDto measurement, 
            List<ComplianceViolation> violations, List<ComplianceWarningDto> warnings)
        {
            if (violations.Count == 0 && warnings.Count == 0)
                return $"Vehicle {measurement.VehicleId} passed all weight compliance checks";

            var summary = $"Vehicle {measurement.VehicleId} compliance summary: ";
            if (violations.Count > 0)
                summary += $"{violations.Count} violation(s) detected. ";
            if (warnings.Count > 0)
                summary += $"{warnings.Count} warning(s) issued. ";

            return summary.Trim();
        }

        private string GenerateLicenseComplianceSummary(string driverId, 
            List<ComplianceViolation> violations, List<ComplianceWarningDto> warnings)
        {
            if (violations.Count == 0 && warnings.Count == 0)
                return $"Driver {driverId} license is compliant";

            var summary = $"Driver {driverId} license compliance: ";
            if (violations.Count > 0)
                summary += $"{violations.Count} violation(s) detected. ";
            if (warnings.Count > 0)
                summary += $"{warnings.Count} warning(s) issued. ";

            return summary.Trim();
        }

        private string GenerateRouteComplianceSummary(string routeId, string vehicleId, 
            List<ComplianceViolation> violations, List<ComplianceWarningDto> warnings)
        {
            if (violations.Count == 0 && warnings.Count == 0)
                return $"Route {routeId} is compliant for vehicle {vehicleId}";

            var summary = $"Route {routeId} compliance for vehicle {vehicleId}: ";
            if (violations.Count > 0)
                summary += $"{violations.Count} violation(s) detected. ";
            if (warnings.Count > 0)
                summary += $"{warnings.Count} warning(s) issued. ";

            return summary.Trim();
        }

        private string GenerateProductComplianceSummary(string productId, string vehicleId, 
            List<ComplianceViolation> violations)
        {
            if (violations.Count == 0)
                return $"Product {productId} transport is compliant for vehicle {vehicleId}";

            return $"Product {productId} transport compliance for vehicle {vehicleId}: {violations.Count} violation(s) detected";
        }

        private string GenerateComprehensiveComplianceSummary(TransactionComplianceDto transaction, 
            List<ComplianceViolation> violations, List<ComplianceWarningDto> warnings)
        {
            if (violations.Count == 0 && warnings.Count == 0)
                return $"Transaction {transaction.TransactionId} is fully compliant";

            var summary = $"Transaction {transaction.TransactionId} compliance summary: ";
            if (violations.Count > 0)
                summary += $"{violations.Count} total violation(s), ";
            if (warnings.Count > 0)
                summary += $"{warnings.Count} warning(s), ";

            var violationsByType = violations.GroupBy(v => v.ViolationType).ToDictionary(g => g.Key, g => g.Count());
            if (violationsByType.Any())
            {
                summary += "Types: " + string.Join(", ", violationsByType.Select(kvp => $"{kvp.Key}({kvp.Value})"));
            }

            return summary.Trim().TrimEnd(',');
        }

        public async Task<ComplianceResultDto> EvaluateRulesAsync(string entityType, string entityId, List<object> rules)
        {
            // Basic implementation - can be enhanced later
            var result = new ComplianceResultDto
            {
                IsCompliant = true,
                Status = "COMPLIANT",
                Severity = "LOW",
                Summary = "Rules evaluation completed",
                CheckedAt = DateTime.UtcNow,
                ComplianceId = Guid.NewGuid().ToString(),
                EntityType = entityType,
                EntityId = entityId,
                ComplianceScore = 100.0,
                CheckDate = DateTime.UtcNow
            };
            
            return await Task.FromResult(result);
        }
    }
}