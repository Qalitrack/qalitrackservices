using OperationalDataService.Core.DTOs;
using OperationalDataService.Core.Entities;

namespace OperationalDataService.Core.Interfaces;

public interface IOperationalDataService
{
    // Operational Management
    Task<OperationalDto> CreateOperationAsync(CreateOperationalRequest request);
    Task<OperationalDto?> GetOperationAsync(string operationId);
    Task<OperationalDto> UpdateOperationAsync(string operationId, UpdateOperationalRequest request);
    Task<bool> DeleteOperationAsync(string operationId);
    Task<List<OperationalDto>> GetOperationsByOrganizationAsync(string organizationId);
    Task<List<OperationalDto>> GetActiveOperationsAsync();
    Task<List<OperationalDto>> GetOperationsByStatusAsync(OperationStatus status);
    Task<List<OperationalDto>> GetOperationsByTypeAsync(OperationType operationType);
    
    // Operation State Management
    Task<bool> StartOperationAsync(string operationId, string userId);
    Task<bool> PauseOperationAsync(string operationId, string userId);
    Task<bool> ResumeOperationAsync(string operationId, string userId);
    Task<bool> CompleteOperationAsync(string operationId, string userId);
    Task<bool> CancelOperationAsync(string operationId, string userId, string reason);
    
    // Hierarchical Operations
    Task<List<OperationalDto>> GetSubOperationsAsync(string parentOperationId);
    Task<OperationalDto?> GetParentOperationAsync(string operationId);
    Task<bool> AddSubOperationAsync(string parentOperationId, string subOperationId);
    Task<bool> RemoveSubOperationAsync(string parentOperationId, string subOperationId);
}