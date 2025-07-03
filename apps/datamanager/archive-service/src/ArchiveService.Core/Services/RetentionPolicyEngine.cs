using ArchiveService.Core.DTOs;
using ArchiveService.Core.Entities;
using ArchiveService.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ArchiveService.Core.Services
{
    public class RetentionPolicyEngine : IRetentionPolicyEngine
    {
        private readonly IRetentionPolicyRepository _retentionPolicyRepository;
        private readonly IArchivalEngine _archivalEngine;
        private readonly ILogger<RetentionPolicyEngine> _logger;

        public RetentionPolicyEngine(
            IRetentionPolicyRepository retentionPolicyRepository,
            IArchivalEngine archivalEngine,
            ILogger<RetentionPolicyEngine> logger)
        {
            _retentionPolicyRepository = retentionPolicyRepository;
            _archivalEngine = archivalEngine;
            _logger = logger;
        }

        public async Task<IEnumerable<RetentionPolicyDto>> GetPoliciesAsync(CancellationToken cancellationToken = default)
        {
            var policies = await _retentionPolicyRepository.GetAllAsync(cancellationToken);
            return policies.Select(MapToDto);
        }

        public async Task<RetentionPolicyDto?> GetPolicyAsync(int policyId, CancellationToken cancellationToken = default)
        {
            var policy = await _retentionPolicyRepository.GetByIdAsync(policyId, cancellationToken);
            return policy != null ? MapToDto(policy) : null;
        }

        public async Task<RetentionPolicyDto> CreatePolicyAsync(CreateRetentionPolicyDto request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new retention policy: {PolicyName}", request.Name);

            try
            {
                // Validate policy
                var isValid = await ValidatePolicyAsync(request, cancellationToken);
                if (!isValid)
                {
                    throw new ArgumentException("Invalid policy configuration");
                }

                var policy = new RetentionPolicy
                {
                    Name = request.Name,
                    Description = request.Description,
                    EntityType = request.EntityType,
                    Category = request.Category,
                    RetentionDays = request.RetentionDays,
                    ArchiveAfterDays = request.ArchiveAfterDays,
                    DeleteAfterDays = request.DeleteAfterDays,
                    StorageTier = request.StorageTier,
                    CompressionType = request.CompressionType,
                    IsActive = request.IsActive,
                    IsAutomatic = request.IsAutomatic,
                    ExecutionSchedule = request.ExecutionSchedule,
                    FilterCriteria = JsonSerializer.Serialize(request.FilterCriteria),
                    OrganizationIds = string.Join(",", request.OrganizationIds),
                    Priority = request.Priority,
                    CreatedBy = "SYSTEM", // TODO: Get from current user context
                    UpdatedBy = "SYSTEM",
                    Notes = request.Notes,
                    NextExecution = CalculateNextExecution(request.ExecutionSchedule)
                };

                var createdPolicy = await _retentionPolicyRepository.AddAsync(policy, cancellationToken);
                
                _logger.LogInformation("Retention policy created successfully with ID: {PolicyId}", createdPolicy.Id);
                return MapToDto(createdPolicy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating retention policy: {PolicyName}", request.Name);
                throw;
            }
        }

        public async Task<RetentionPolicyDto> UpdatePolicyAsync(int policyId, UpdateRetentionPolicyDto request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating retention policy with ID: {PolicyId}", policyId);

            try
            {
                var policy = await _retentionPolicyRepository.GetByIdAsync(policyId, cancellationToken);
                if (policy == null)
                {
                    throw new InvalidOperationException($"Retention policy with ID {policyId} not found");
                }

                // Update only provided fields
                if (!string.IsNullOrEmpty(request.Name))
                    policy.Name = request.Name;
                
                if (!string.IsNullOrEmpty(request.Description))
                    policy.Description = request.Description;
                
                if (request.RetentionDays.HasValue)
                    policy.RetentionDays = request.RetentionDays.Value;
                
                if (request.ArchiveAfterDays.HasValue)
                    policy.ArchiveAfterDays = request.ArchiveAfterDays.Value;
                
                if (request.DeleteAfterDays.HasValue)
                    policy.DeleteAfterDays = request.DeleteAfterDays.Value;
                
                if (!string.IsNullOrEmpty(request.StorageTier))
                    policy.StorageTier = request.StorageTier;
                
                if (!string.IsNullOrEmpty(request.CompressionType))
                    policy.CompressionType = request.CompressionType;
                
                if (request.IsActive.HasValue)
                    policy.IsActive = request.IsActive.Value;
                
                if (request.IsAutomatic.HasValue)
                    policy.IsAutomatic = request.IsAutomatic.Value;
                
                if (!string.IsNullOrEmpty(request.ExecutionSchedule))
                {
                    policy.ExecutionSchedule = request.ExecutionSchedule;
                    policy.NextExecution = CalculateNextExecution(request.ExecutionSchedule);
                }
                
                if (request.FilterCriteria != null)
                    policy.FilterCriteria = JsonSerializer.Serialize(request.FilterCriteria);
                
                if (request.OrganizationIds != null)
                    policy.OrganizationIds = string.Join(",", request.OrganizationIds);
                
                if (!string.IsNullOrEmpty(request.Priority))
                    policy.Priority = request.Priority;
                
                if (!string.IsNullOrEmpty(request.Notes))
                    policy.Notes = request.Notes;

                policy.UpdatedBy = "SYSTEM"; // TODO: Get from current user context
                policy.UpdatedAt = DateTime.UtcNow;

                var updatedPolicy = await _retentionPolicyRepository.UpdateAsync(policy, cancellationToken);
                
                _logger.LogInformation("Retention policy updated successfully: {PolicyId}", policyId);
                return MapToDto(updatedPolicy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating retention policy: {PolicyId}", policyId);
                throw;
            }
        }

        public async Task<bool> DeletePolicyAsync(int policyId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting retention policy with ID: {PolicyId}", policyId);

            try
            {
                var result = await _retentionPolicyRepository.DeleteAsync(policyId, cancellationToken);
                
                if (result)
                {
                    _logger.LogInformation("Retention policy deleted successfully: {PolicyId}", policyId);
                }
                else
                {
                    _logger.LogWarning("Retention policy not found for deletion: {PolicyId}", policyId);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting retention policy: {PolicyId}", policyId);
                throw;
            }
        }

        public async Task<bool> ExecutePolicyAsync(int policyId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Executing retention policy with ID: {PolicyId}", policyId);

            try
            {
                var policy = await _retentionPolicyRepository.GetByIdAsync(policyId, cancellationToken);
                if (policy == null || !policy.IsActive)
                {
                    _logger.LogWarning("Retention policy not found or inactive: {PolicyId}", policyId);
                    return false;
                }

                await ExecutePolicyInternal(policy, cancellationToken);
                
                // Update last execution time
                policy.LastExecuted = DateTime.UtcNow;
                policy.NextExecution = CalculateNextExecution(policy.ExecutionSchedule, DateTime.UtcNow);
                policy.UpdatedAt = DateTime.UtcNow;
                await _retentionPolicyRepository.UpdateAsync(policy, cancellationToken);

                _logger.LogInformation("Retention policy executed successfully: {PolicyId}", policyId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing retention policy: {PolicyId}", policyId);
                throw;
            }
        }

        public async Task<bool> ExecuteAllPoliciesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Executing all active retention policies");

            try
            {
                var duePolicies = await _retentionPolicyRepository.GetDuePoliciesAsync(DateTime.UtcNow, cancellationToken);
                var successCount = 0;
                var totalCount = 0;

                foreach (var policy in duePolicies)
                {
                    totalCount++;
                    try
                    {
                        var success = await ExecutePolicyAsync(policy.Id, cancellationToken);
                        if (success) successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing policy {PolicyId} during batch execution", policy.Id);
                    }
                }

                _logger.LogInformation("Batch policy execution completed. {SuccessCount}/{TotalCount} policies executed successfully", successCount, totalCount);
                return successCount == totalCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during batch policy execution");
                throw;
            }
        }

        public async Task<IEnumerable<RetentionPolicyDto>> GetActivePoliciesAsync(CancellationToken cancellationToken = default)
        {
            var policies = await _retentionPolicyRepository.GetActivePoliciesAsync(cancellationToken);
            return policies.Select(MapToDto);
        }

        public async Task<IEnumerable<RetentionPolicyDto>> GetPoliciesByEntityTypeAsync(string entityType, CancellationToken cancellationToken = default)
        {
            var policies = await _retentionPolicyRepository.GetByEntityTypeAsync(entityType, cancellationToken);
            return policies.Select(MapToDto);
        }

        public async Task<bool> ValidatePolicyAsync(CreateRetentionPolicyDto policy, CancellationToken cancellationToken = default)
        {
            try
            {
                // Basic validation
                if (string.IsNullOrWhiteSpace(policy.Name))
                    return false;

                if (string.IsNullOrWhiteSpace(policy.EntityType))
                    return false;

                if (policy.RetentionDays < 0 || policy.ArchiveAfterDays < 0 || policy.DeleteAfterDays < 0)
                    return false;

                if (policy.ArchiveAfterDays > policy.RetentionDays)
                    return false;

                if (policy.DeleteAfterDays > 0 && policy.DeleteAfterDays < policy.ArchiveAfterDays)
                    return false;

                // Validate entity type
                var validEntityTypes = new[] { "TRANSACTIONS", "WEIGHT_MEASUREMENTS", "COMPLIANCE_RECORDS" };
                if (!validEntityTypes.Contains(policy.EntityType))
                    return false;

                // Validate storage tier
                var validStorageTiers = new[] { "HOT", "WARM", "COLD" };
                if (!validStorageTiers.Contains(policy.StorageTier))
                    return false;

                // Validate compression type
                var validCompressionTypes = new[] { "GZIP", "DEFLATE" };
                if (!validCompressionTypes.Contains(policy.CompressionType))
                    return false;

                // Validate execution schedule
                var validSchedules = new[] { "DAILY", "WEEKLY", "MONTHLY" };
                if (!validSchedules.Contains(policy.ExecutionSchedule))
                    return false;

                await Task.CompletedTask; // Placeholder for async validation
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating retention policy");
                return false;
            }
        }

        public async Task<DateTime?> GetNextExecutionTimeAsync(int policyId, CancellationToken cancellationToken = default)
        {
            var policy = await _retentionPolicyRepository.GetByIdAsync(policyId, cancellationToken);
            return policy?.NextExecution;
        }

        public async Task<Dictionary<string, object>> GetPolicyStatisticsAsync(int policyId, CancellationToken cancellationToken = default)
        {
            var policy = await _retentionPolicyRepository.GetByIdAsync(policyId, cancellationToken);
            if (policy == null)
            {
                return new Dictionary<string, object>();
            }

            // TODO: Implement actual statistics calculation
            return new Dictionary<string, object>
            {
                ["policyId"] = policyId,
                ["name"] = policy.Name,
                ["entityType"] = policy.EntityType,
                ["isActive"] = policy.IsActive,
                ["lastExecuted"] = policy.LastExecuted,
                ["nextExecution"] = policy.NextExecution,
                ["totalExecutions"] = 0, // TODO: Track execution count
                ["archiveCount"] = 0, // TODO: Count related archives
                ["totalArchivedSize"] = 0L // TODO: Calculate total size
            };
        }

        private async Task ExecutePolicyInternal(RetentionPolicy policy, CancellationToken cancellationToken)
        {
            try
            {
                var filterCriteria = !string.IsNullOrEmpty(policy.FilterCriteria)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(policy.FilterCriteria) ?? new Dictionary<string, object>()
                    : new Dictionary<string, object>();

                var organizationIds = !string.IsNullOrEmpty(policy.OrganizationIds)
                    ? policy.OrganizationIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    : Array.Empty<string>();

                // Archive data if specified
                if (policy.ArchiveAfterDays > 0)
                {
                    var archiveDate = DateTime.UtcNow.AddDays(-policy.ArchiveAfterDays);
                    var archiveRequest = new ArchiveRequestDto
                    {
                        EntityType = policy.EntityType,
                        StartDate = DateTime.MinValue,
                        EndDate = archiveDate,
                        OrganizationIds = organizationIds,
                        CustomFilters = filterCriteria,
                        StorageTier = policy.StorageTier,
                        CompressionType = policy.CompressionType,
                        Description = $"Automated archival by policy: {policy.Name}",
                        DeleteAfterArchive = true,
                        CreateIndex = true,
                        Priority = policy.Priority,
                        RequestedBy = "RETENTION_POLICY_ENGINE"
                    };

                    switch (policy.EntityType)
                    {
                        case "TRANSACTIONS":
                            await _archivalEngine.ArchiveTransactionsAsync(archiveRequest, cancellationToken);
                            break;
                        case "WEIGHT_MEASUREMENTS":
                            await _archivalEngine.ArchiveWeightMeasurementsAsync(archiveRequest, cancellationToken);
                            break;
                        case "COMPLIANCE_RECORDS":
                            await _archivalEngine.ArchiveComplianceRecordsAsync(archiveRequest, cancellationToken);
                            break;
                    }
                }

                // Delete old archived data if specified
                if (policy.DeleteAfterDays > 0)
                {
                    var deleteDate = DateTime.UtcNow.AddDays(-policy.DeleteAfterDays);
                    // TODO: Implement deletion of old archived data
                    _logger.LogInformation("Would delete archived data older than {DeleteDate} for policy {PolicyId}", deleteDate, policy.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing retention policy {PolicyId}", policy.Id);
                throw;
            }
        }

        private DateTime? CalculateNextExecution(string schedule, DateTime? baseTime = null)
        {
            var now = baseTime ?? DateTime.UtcNow;
            
            return schedule.ToUpper() switch
            {
                "DAILY" => now.AddDays(1).Date.AddHours(2), // 2 AM next day
                "WEEKLY" => now.AddDays(7 - (int)now.DayOfWeek).Date.AddHours(2), // Next Sunday at 2 AM
                "MONTHLY" => new DateTime(now.Year, now.Month, 1).AddMonths(1).AddHours(2), // First day of next month at 2 AM
                _ => null
            };
        }

        private RetentionPolicyDto MapToDto(RetentionPolicy policy)
        {
            var filterCriteria = !string.IsNullOrEmpty(policy.FilterCriteria)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(policy.FilterCriteria) ?? new Dictionary<string, object>()
                : new Dictionary<string, object>();

            var organizationIds = !string.IsNullOrEmpty(policy.OrganizationIds)
                ? policy.OrganizationIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                : Array.Empty<string>();

            return new RetentionPolicyDto
            {
                Id = policy.Id,
                Name = policy.Name,
                Description = policy.Description,
                EntityType = policy.EntityType,
                Category = policy.Category,
                RetentionDays = policy.RetentionDays,
                ArchiveAfterDays = policy.ArchiveAfterDays,
                DeleteAfterDays = policy.DeleteAfterDays,
                StorageTier = policy.StorageTier,
                CompressionType = policy.CompressionType,
                IsActive = policy.IsActive,
                IsAutomatic = policy.IsAutomatic,
                ExecutionSchedule = policy.ExecutionSchedule,
                LastExecuted = policy.LastExecuted,
                NextExecution = policy.NextExecution,
                FilterCriteria = filterCriteria,
                OrganizationIds = organizationIds,
                Priority = policy.Priority,
                CreatedBy = policy.CreatedBy,
                UpdatedBy = policy.UpdatedBy,
                CreatedAt = policy.CreatedAt,
                UpdatedAt = policy.UpdatedAt,
                Notes = policy.Notes,
                ArchiveCount = 0, // TODO: Calculate from related archives
                TotalArchivedSize = 0L // TODO: Calculate from related archives
            };
        }
    }
}