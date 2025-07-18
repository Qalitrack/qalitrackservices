using OperationalDataService.Core.Entities;

namespace OperationalDataService.Core.Interfaces;

public interface IProcessRepository : IRepository<Process>
{
    Task<List<Process>> GetByOperationAsync(string operationId);
    Task<List<Process>> GetByStatusAsync(ProcessStatus status);
    Task<List<Process>> GetByTypeAsync(ProcessType processType);
    Task<List<Process>> GetByAssignedUserAsync(string userId);
    Task<List<Process>> GetAutomatedProcessesAsync();
    Task<List<Process>> GetProcessesRequiringApprovalAsync();
    Task<Process?> GetNextProcessAsync(string processId);
    Task<Process?> GetPreviousProcessAsync(string processId);
    Task<List<Process>> GetProcessSequenceAsync(string operationId);
    Task<List<Process>> GetParallelProcessesAsync(string operationId);
    Task<List<Process>> GetFailedProcessesAsync();
    Task<List<Process>> GetProcessesByProgressAsync(decimal minProgress, decimal maxProgress);
}