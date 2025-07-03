using ComplianceService.Core.DTOs;
using ComplianceService.Core.Entities;
using ComplianceService.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ComplianceService.Core.Services
{
    public class ViolationDetectionService : IViolationDetectionService
    {
        private readonly IComplianceRepository _repository;
        private readonly ILogger<ViolationDetectionService> _logger;

        public ViolationDetectionService(IComplianceRepository repository, ILogger<ViolationDetectionService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<ComplianceViolation>> DetectWeightViolationsAsync(WeightMeasurementDto measurement)
        {
            _logger.LogInformation("Detecting weight violations for vehicle {VehicleId}", measurement.VehicleId);

            var violations = new List<ComplianceViolation>();

            try
            {
                // Get applicable weight limits
                var weightLimits = await _repository.GetWeightLimitsByVehicleTypeAsync(measurement.VehicleType) ??
                                 (await _repository.GetActiveWeightLimitsAsync()).FirstOrDefault();

                if (weightLimits == null)
                {
                    _logger.LogWarning("No weight limits found for vehicle type {VehicleType}", measurement.VehicleType);
                    return violations;
                }

                // Check gross weight violation
                if (measurement.GrossWeight > weightLimits.MaxGrossWeight)
                {
                    var grossViolation = CreateWeightViolation(
                        "GROSS_WEIGHT_EXCEEDED",
                        measurement,
                        measurement.GrossWeight,
                        weightLimits.MaxGrossWeight,
                        weightLimits.OverweightPenaltyRate,
                        "HIGH",
                        $"Gross weight {measurement.GrossWeight} KG exceeds maximum limit of {weightLimits.MaxGrossWeight} KG"
                    );
                    violations.Add(grossViolation);
                }

                // Check axle weight violations
                foreach (var axleWeight in measurement.AxleWeights)
                {
                    var maxAxleWeight = DetermineMaxAxleWeight(axleWeight, weightLimits);
                    if (axleWeight.Weight > maxAxleWeight)
                    {
                        var axleViolation = CreateWeightViolation(
                            "AXLE_WEIGHT_EXCEEDED",
                            measurement,
                            axleWeight.Weight,
                            maxAxleWeight,
                            weightLimits.OverweightPenaltyRate,
                            "MEDIUM",
                            $"Axle {axleWeight.AxleNumber} weight {axleWeight.Weight} KG exceeds maximum limit of {maxAxleWeight} KG"
                        );
                        violations.Add(axleViolation);
                    }
                }

                // Check axle count violation
                if (measurement.AxleCount > weightLimits.MaxAxleCount)
                {
                    var axleCountViolation = CreateWeightViolation(
                        "AXLE_COUNT_EXCEEDED",
                        measurement,
                        measurement.AxleCount,
                        weightLimits.MaxAxleCount,
                        0, // No penalty for axle count, just violation
                        "MEDIUM",
                        $"Vehicle has {measurement.AxleCount} axles, exceeding maximum allowed {weightLimits.MaxAxleCount}"
                    );
                    violations.Add(axleCountViolation);
                }

                _logger.LogInformation("Weight violation detection completed. Found {ViolationCount} violations", violations.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting weight violations for vehicle {VehicleId}", measurement.VehicleId);
            }

            return violations;
        }

        public async Task<List<ComplianceViolation>> DetectLicenseViolationsAsync(string driverId)
        {
            _logger.LogInformation("Detecting license violations for driver {DriverId}", driverId);

            var violations = new List<ComplianceViolation>();

            try
            {
                var license = await _repository.GetLicenseByDriverIdAsync(driverId);
                if (license == null)
                {
                    violations.Add(CreateLicenseViolation(
                        "LICENSE_NOT_FOUND",
                        driverId,
                        "No license record found for driver",
                        "CRITICAL"
                    ));
                    return violations;
                }

                // Check if license is expired
                if (license.IsExpired)
                {
                    violations.Add(CreateLicenseViolation(
                        "LICENSE_EXPIRED",
                        driverId,
                        $"Driver license expired on {license.ExpiryDate:yyyy-MM-dd}",
                        "CRITICAL",
                        license.LicenseNumber
                    ));
                }

                // Check license status violations
                if (license.Status == "SUSPENDED")
                {
                    violations.Add(CreateLicenseViolation(
                        "LICENSE_SUSPENDED",
                        driverId,
                        "Driver license is currently suspended",
                        "CRITICAL",
                        license.LicenseNumber
                    ));
                }
                else if (license.Status == "REVOKED")
                {
                    violations.Add(CreateLicenseViolation(
                        "LICENSE_REVOKED",
                        driverId,
                        "Driver license has been revoked",
                        "CRITICAL",
                        license.LicenseNumber
                    ));
                }

                _logger.LogInformation("License violation detection completed. Found {ViolationCount} violations", violations.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting license violations for driver {DriverId}", driverId);
            }

            return violations;
        }

        public async Task<List<ComplianceViolation>> DetectRouteViolationsAsync(string routeId, string vehicleId, DateTime timestamp)
        {
            _logger.LogInformation("Detecting route violations for route {RouteId}, vehicle {VehicleId}", routeId, vehicleId);

            var violations = new List<ComplianceViolation>();

            try
            {
                var restrictions = await _repository.GetRouteRestrictionsAsync(routeId);
                var activeRestrictions = restrictions.Where(r => r.IsActive).ToList();

                foreach (var restriction in activeRestrictions)
                {
                    // Check time-based restrictions
                    if (IsTimeRestrictionViolated(restriction, timestamp))
                    {
                        violations.Add(CreateRouteViolation(
                            "TIME_RESTRICTION_VIOLATED",
                            routeId,
                            vehicleId,
                            restriction,
                            $"Vehicle traveling on restricted route during prohibited time: {timestamp.TimeOfDay}",
                            restriction.Severity
                        ));
                    }

                    // Check weight-based restrictions
                    // Note: This would require vehicle weight data integration
                    if (restriction.MaxWeight.HasValue)
                    {
                        // This would be integrated with weight measurement data
                        // For now, we'll mark it as a potential check
                        _logger.LogDebug("Weight restriction check needed for route {RouteId}: max {MaxWeight} KG", 
                            routeId, restriction.MaxWeight);
                    }

                    // Check vehicle type restrictions
                    if (!string.IsNullOrEmpty(restriction.VehicleType))
                    {
                        // This would require vehicle details integration
                        _logger.LogDebug("Vehicle type restriction check needed for route {RouteId}: allowed type {VehicleType}", 
                            routeId, restriction.VehicleType);
                    }

                    // Check product type restrictions
                    if (!string.IsNullOrEmpty(restriction.ProductType))
                    {
                        // This would require product details integration
                        _logger.LogDebug("Product type restriction check needed for route {RouteId}: restricted type {ProductType}", 
                            routeId, restriction.ProductType);
                    }
                }

                _logger.LogInformation("Route violation detection completed. Found {ViolationCount} violations", violations.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting route violations for route {RouteId}, vehicle {VehicleId}", routeId, vehicleId);
            }

            return violations;
        }

        public async Task<List<ComplianceViolation>> DetectProductViolationsAsync(string productId, string vehicleId)
        {
            _logger.LogInformation("Detecting product violations for product {ProductId}, vehicle {VehicleId}", productId, vehicleId);

            var violations = new List<ComplianceViolation>();

            try
            {
                // Product compliance rules would be defined in the rules engine
                // This could include:
                // - Hazardous material transport requirements
                // - Vehicle compatibility checks
                // - Special permit requirements
                // - Temperature control requirements

                var productRules = await _repository.GetRulesByTypeAsync("PRODUCT");
                var activeRules = productRules.Where(r => r.IsActive).ToList();

                foreach (var rule in activeRules)
                {
                    // Evaluate product-specific rules
                    var ruleConfig = JsonSerializer.Deserialize<Dictionary<string, object>>(rule.ConfigurationJson);
                    
                    // Example: Check if hazardous products are being transported in appropriate vehicles
                    if (rule.Category == "HAZARDOUS_TRANSPORT")
                    {
                        // This would require integration with product and vehicle master data
                        _logger.LogDebug("Hazardous transport compliance check needed for product {ProductId}, vehicle {VehicleId}", 
                            productId, vehicleId);
                    }
                }

                _logger.LogInformation("Product violation detection completed. Found {ViolationCount} violations", violations.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting product violations for product {ProductId}, vehicle {VehicleId}", productId, vehicleId);
            }

            return violations;
        }

        public async Task<List<ComplianceViolation>> DetectAllViolationsAsync(TransactionComplianceDto transaction)
        {
            _logger.LogInformation("Detecting all violations for transaction {TransactionId}", transaction.TransactionId);

            var allViolations = new List<ComplianceViolation>();

            try
            {
                // Weight violations
                if (transaction.WeightMeasurement != null)
                {
                    var weightViolations = await DetectWeightViolationsAsync(transaction.WeightMeasurement);
                    allViolations.AddRange(weightViolations);
                }

                // License violations
                if (!string.IsNullOrEmpty(transaction.DriverId))
                {
                    var licenseViolations = await DetectLicenseViolationsAsync(transaction.DriverId);
                    allViolations.AddRange(licenseViolations);
                }

                // Route violations
                if (!string.IsNullOrEmpty(transaction.RouteId) && !string.IsNullOrEmpty(transaction.VehicleId))
                {
                    var routeViolations = await DetectRouteViolationsAsync(transaction.RouteId, transaction.VehicleId, transaction.TransactionTime);
                    allViolations.AddRange(routeViolations);
                }

                // Product violations
                if (!string.IsNullOrEmpty(transaction.ProductId) && !string.IsNullOrEmpty(transaction.VehicleId))
                {
                    var productViolations = await DetectProductViolationsAsync(transaction.ProductId, transaction.VehicleId);
                    allViolations.AddRange(productViolations);
                }

                // Set common properties for all violations
                foreach (var violation in allViolations)
                {
                    violation.TransactionId = transaction.TransactionId;
                    violation.OrganizationId = transaction.OrganizationId;
                    violation.DetectedAt = DateTime.UtcNow;
                }

                _logger.LogInformation("All violation detection completed for transaction {TransactionId}. Found {ViolationCount} total violations", 
                    transaction.TransactionId, allViolations.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting all violations for transaction {TransactionId}", transaction.TransactionId);
            }

            return allViolations;
        }

        private ComplianceViolation CreateWeightViolation(string violationType, WeightMeasurementDto measurement,
            decimal actualValue, decimal limitValue, decimal penaltyRate, string severity, string description)
        {
            var excessValue = actualValue - limitValue;
            var penaltyAmount = excessValue * penaltyRate;

            return new ComplianceViolation
            {
                ViolationType = violationType,
                TransactionId = measurement.TransactionId,
                VehicleId = measurement.VehicleId,
                DriverId = measurement.DriverId,
                WeighbridgeId = measurement.WeighbridgeId,
                OrganizationId = measurement.OrganizationId,
                Status = "ACTIVE",
                Severity = severity,
                ActualValue = actualValue,
                LimitValue = limitValue,
                ExcessValue = Math.Max(0, excessValue),
                Unit = measurement.Unit,
                PenaltyAmount = Math.Max(0, penaltyAmount),
                PenaltyCurrency = "KES",
                Description = description,
                Details = JsonSerializer.Serialize(new
                {
                    MeasurementId = measurement.Id,
                    MeasurementTime = measurement.MeasuredAt,
                    WeighbridgeId = measurement.WeighbridgeId,
                    VehicleType = measurement.VehicleType,
                    AxleWeights = measurement.AxleWeights
                }),
                DetectedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private ComplianceViolation CreateLicenseViolation(string violationType, string driverId, 
            string description, string severity, string licenseNumber = "")
        {
            return new ComplianceViolation
            {
                ViolationType = violationType,
                DriverId = driverId,
                Status = "ACTIVE",
                Severity = severity,
                Description = description,
                Details = JsonSerializer.Serialize(new
                {
                    LicenseNumber = licenseNumber,
                    CheckTime = DateTime.UtcNow
                }),
                DetectedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private ComplianceViolation CreateRouteViolation(string violationType, string routeId, string vehicleId,
            RouteRestrictions restriction, string description, string severity)
        {
            return new ComplianceViolation
            {
                ViolationType = violationType,
                VehicleId = vehicleId,
                Status = "ACTIVE",
                Severity = severity,
                Description = description,
                PenaltyAmount = restriction.ViolationPenalty,
                PenaltyCurrency = restriction.PenaltyCurrency,
                Details = JsonSerializer.Serialize(new
                {
                    RouteId = routeId,
                    RestrictionId = restriction.Id,
                    RestrictionType = restriction.RestrictionType,
                    RestrictedFromTime = restriction.RestrictedFromTime,
                    RestrictedToTime = restriction.RestrictedToTime,
                    CheckTime = DateTime.UtcNow
                }),
                DetectedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private decimal DetermineMaxAxleWeight(AxleWeightDto axleWeight, WeightLimits weightLimits)
        {
            return axleWeight.Position?.ToUpper() switch
            {
                "FRONT" => weightLimits.MaxFrontAxleWeight,
                "REAR" => weightLimits.MaxRearAxleWeight,
                _ => weightLimits.MaxAxleWeight
            };
        }

        private bool IsTimeRestrictionViolated(RouteRestrictions restriction, DateTime timestamp)
        {
            if (!restriction.RestrictedFromTime.HasValue || !restriction.RestrictedToTime.HasValue)
                return false;

            var currentTime = timestamp.TimeOfDay;
            var restrictedFrom = restriction.RestrictedFromTime.Value;
            var restrictedTo = restriction.RestrictedToTime.Value;

            // Handle time ranges that cross midnight
            if (restrictedFrom <= restrictedTo)
            {
                return currentTime >= restrictedFrom && currentTime <= restrictedTo;
            }
            else
            {
                return currentTime >= restrictedFrom || currentTime <= restrictedTo;
            }
        }
    }
}