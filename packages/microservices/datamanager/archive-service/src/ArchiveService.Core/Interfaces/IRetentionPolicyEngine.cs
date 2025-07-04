using ArchiveService.Core.DTOs;
using ArchiveService.Core.Entities;

namespace ArchiveService.Core.Interfaces
{
    public interface IRetentionPolicyEngine
    {
        Task<IEnumerable<RetentionPolicyDto>> GetPoliciesAsync(CancellationToken cancellationToken = default);
        Task<RetentionPolicyDto?> GetPolicyAsync(int policyId, CancellationToken cancellationToken = default);
        Task<RetentionPolicyDto> CreatePolicyAsync(CreateRetentionPolicyDto request, CancellationToken cancellationToken = default);
        Task<RetentionPolicyDto> UpdatePolicyAsync(int policyId, UpdateRetentionPolicyDto request, CancellationToken cancellationToken = default);
        Task<bool> DeletePolicyAsync(int policyId, CancellationToken cancellationToken = default);
        Task<bool> ExecutePolicyAsync(int policyId, CancellationToken cancellationToken = default);
        Task<bool> ExecuteAllPoliciesAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<RetentionPolicyDto>> GetActivePoliciesAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<RetentionPolicyDto>> GetPoliciesByEntityTypeAsync(string entityType, CancellationToken cancellationToken = default);
        Task<bool> ValidatePolicyAsync(CreateRetentionPolicyDto policy, CancellationToken cancellationToken = default);
        Task<DateTime?> GetNextExecutionTimeAsync(int policyId, CancellationToken cancellationToken = default);
        Task<Dictionary<string, object>> GetPolicyStatisticsAsync(int policyId, CancellationToken cancellationToken = default);
    }
}