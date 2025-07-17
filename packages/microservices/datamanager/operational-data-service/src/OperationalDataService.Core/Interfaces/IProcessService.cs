using OperationalDataService.Core.DTOs;
using OperationalDataService.Core.Entities;

namespace OperationalDataService.Core.Interfaces;

public interface IProcessService
{
    // Process Management
    Task<ProcessDto> CreateProcessAsync(CreateProcessRequest request);
    Task<ProcessDto?> GetProcessAsync(string processId);
    Task<ProcessDto> UpdateProcessAsync(string processId, UpdateProcessRequest request);
    Task<bool> DeleteProcessAsync(string processId);
    Task<List<ProcessDto>> GetProcessesByOperationAsync(string operationId);
    Task<List<ProcessDto>> GetProcessesByStatusAsync(ProcessStatus status);
    Task<List<ProcessDto>> GetProcessesByTypeAsync(ProcessType processType);
    Task<List<ProcessDto>> GetProcessesByUserAsync(string userId);
    
    // Process Workflow Management
    Task<bool> StartProcessAsync(string processId, string userId);
    Task<bool> PauseProcessAsync(string processId, string userId);
    Task<bool> ResumeProcessAsync(string processId, string userId);
    Task<bool> CompleteProcessAsync(string processId, string userId);
    Task<bool> FailProcessAsync(string processId, string userId, string errorMessage);
    Task<bool> CancelProcessAsync(string processId, string userId, string reason);
    Task<bool> SkipProcessAsync(string processId, string userId, string reason);
    
    // Process Automation
    Task<List<ProcessDto>> GetAutomatedProcessesAsync();
    Task<bool> ExecuteAutomatedProcessAsync(string processId);
    Task<List<ProcessDto>> GetProcessesRequiringApprovalAsync();
    Task<bool> ApproveProcessAsync(string processId, string userId);
    Task<bool> RejectProcessAsync(string processId, string userId, string reason);
    
    // Process Sequencing
    Task<ProcessDto?> GetNextProcessAsync(string processId);
    Task<ProcessDto?> GetPreviousProcessAsync(string processId);
    Task<bool> LinkProcessesAsync(string fromProcessId, string toProcessId);
    Task<bool> UnlinkProcessesAsync(string fromProcessId, string toProcessId);
    Task<List<ProcessDto>> GetProcessSequenceAsync(string operationId);
    
    // Progress Tracking
    Task<bool> UpdateProgressAsync(string processId, decimal progressPercentage);
    Task<ProcessProgressDto> GetProcessProgressAsync(string processId);
    Task<OperationProgressDto> GetOperationProgressAsync(string operationId);
}