using AutoMapper;
using OperationalDataService.Core.DTOs;
using OperationalDataService.Core.Entities;
using OperationalDataService.Core.Interfaces;

namespace OperationalDataService.Core.Services;

public class OperationalDataService : IOperationalDataService
{
    private readonly IOperationalRepository _operationalRepository;
    private readonly IMapper _mapper;

    public OperationalDataService(IOperationalRepository operationalRepository, IMapper mapper)
    {
        _operationalRepository = operationalRepository;
        _mapper = mapper;
    }

    public async Task<OperationalDto> CreateOperationAsync(CreateOperationalRequest request)
    {
        var operation = new Operational
        {
            OperationName = request.OperationName,
            OperationType = request.OperationType,
            OrganizationId = request.OrganizationId,
            Description = request.Description,
            Priority = request.Priority,
            ResponsibleUserId = request.ResponsibleUserId,
            Notes = request.Notes,
            ParentOperationId = request.ParentOperationId,
            Configuration = request.Configuration,
            Metadata = request.Metadata,
            Status = OperationStatus.Pending
        };

        var createdOperation = await _operationalRepository.AddAsync(operation);
        return _mapper.Map<OperationalDto>(createdOperation);
    }

    public async Task<OperationalDto?> GetOperationAsync(string operationId)
    {
        var operation = await _operationalRepository.GetByIdAsync(operationId);
        return operation == null ? null : _mapper.Map<OperationalDto>(operation);
    }

    public async Task<OperationalDto> UpdateOperationAsync(string operationId, UpdateOperationalRequest request)
    {
        var operation = await _operationalRepository.GetByIdAsync(operationId);
        if (operation == null)
            throw new ArgumentException("Operation not found", nameof(operationId));

        if (!string.IsNullOrEmpty(request.OperationName))
            operation.OperationName = request.OperationName;

        if (!string.IsNullOrEmpty(request.Description))
            operation.Description = request.Description;

        if (request.Priority.HasValue)
            operation.Priority = request.Priority.Value;

        if (!string.IsNullOrEmpty(request.ResponsibleUserId))
            operation.ResponsibleUserId = request.ResponsibleUserId;

        if (!string.IsNullOrEmpty(request.Notes))
            operation.Notes = request.Notes;

        if (request.Configuration != null)
            operation.Configuration = request.Configuration;

        if (request.Metadata != null)
            operation.Metadata = request.Metadata;

        operation.UpdatedAt = DateTime.UtcNow;

        var updatedOperation = await _operationalRepository.UpdateAsync(operation);
        return _mapper.Map<OperationalDto>(updatedOperation);
    }

    public async Task<bool> DeleteOperationAsync(string operationId)
    {
        return await _operationalRepository.DeleteByIdAsync(operationId);
    }

    public async Task<List<OperationalDto>> GetOperationsByOrganizationAsync(string organizationId)
    {
        var operations = await _operationalRepository.GetByOrganizationAsync(organizationId);
        return _mapper.Map<List<OperationalDto>>(operations);
    }

    public async Task<List<OperationalDto>> GetActiveOperationsAsync()
    {
        var operations = await _operationalRepository.GetByStatusAsync(OperationStatus.Active);
        return _mapper.Map<List<OperationalDto>>(operations);
    }

    public async Task<List<OperationalDto>> GetOperationsByStatusAsync(OperationStatus status)
    {
        var operations = await _operationalRepository.GetByStatusAsync(status);
        return _mapper.Map<List<OperationalDto>>(operations);
    }

    public async Task<List<OperationalDto>> GetOperationsByTypeAsync(OperationType operationType)
    {
        var operations = await _operationalRepository.GetByTypeAsync(operationType);
        return _mapper.Map<List<OperationalDto>>(operations);
    }

    public async Task<bool> StartOperationAsync(string operationId, string userId)
    {
        var operation = await _operationalRepository.GetByIdAsync(operationId);
        if (operation == null || operation.Status != OperationStatus.Pending)
            return false;

        operation.Status = OperationStatus.Active;
        operation.StartTime = DateTime.UtcNow;
        operation.UpdatedBy = userId;
        operation.UpdatedAt = DateTime.UtcNow;

        await _operationalRepository.UpdateAsync(operation);
        return true;
    }

    public async Task<bool> PauseOperationAsync(string operationId, string userId)
    {
        var operation = await _operationalRepository.GetByIdAsync(operationId);
        if (operation == null || operation.Status != OperationStatus.Active)
            return false;

        operation.Status = OperationStatus.Paused;
        operation.UpdatedBy = userId;
        operation.UpdatedAt = DateTime.UtcNow;

        await _operationalRepository.UpdateAsync(operation);
        return true;
    }

    public async Task<bool> ResumeOperationAsync(string operationId, string userId)
    {
        var operation = await _operationalRepository.GetByIdAsync(operationId);
        if (operation == null || operation.Status != OperationStatus.Paused)
            return false;

        operation.Status = OperationStatus.Active;
        operation.UpdatedBy = userId;
        operation.UpdatedAt = DateTime.UtcNow;

        await _operationalRepository.UpdateAsync(operation);
        return true;
    }

    public async Task<bool> CompleteOperationAsync(string operationId, string userId)
    {
        var operation = await _operationalRepository.GetByIdAsync(operationId);
        if (operation == null)
            return false;

        operation.Status = OperationStatus.Completed;
        operation.EndTime = DateTime.UtcNow;
        operation.UpdatedBy = userId;
        operation.UpdatedAt = DateTime.UtcNow;

        await _operationalRepository.UpdateAsync(operation);
        return true;
    }

    public async Task<bool> CancelOperationAsync(string operationId, string userId, string reason)
    {
        var operation = await _operationalRepository.GetByIdAsync(operationId);
        if (operation == null)
            return false;

        operation.Status = OperationStatus.Cancelled;
        operation.EndTime = DateTime.UtcNow;
        operation.Notes = $"{operation.Notes}\n[CANCELLED] {reason}";
        operation.UpdatedBy = userId;
        operation.UpdatedAt = DateTime.UtcNow;

        await _operationalRepository.UpdateAsync(operation);
        return true;
    }

    public async Task<List<OperationalDto>> GetSubOperationsAsync(string parentOperationId)
    {
        var subOperations = await _operationalRepository.GetSubOperationsAsync(parentOperationId);
        return _mapper.Map<List<OperationalDto>>(subOperations);
    }

    public async Task<OperationalDto?> GetParentOperationAsync(string operationId)
    {
        var operation = await _operationalRepository.GetByIdAsync(operationId);
        if (operation?.ParentOperationId == null)
            return null;

        var parentOperation = await _operationalRepository.GetByIdAsync(operation.ParentOperationId);
        return parentOperation == null ? null : _mapper.Map<OperationalDto>(parentOperation);
    }

    public async Task<bool> AddSubOperationAsync(string parentOperationId, string subOperationId)
    {
        var subOperation = await _operationalRepository.GetByIdAsync(subOperationId);
        if (subOperation == null)
            return false;

        subOperation.ParentOperationId = parentOperationId;
        subOperation.UpdatedAt = DateTime.UtcNow;

        await _operationalRepository.UpdateAsync(subOperation);
        return true;
    }

    public async Task<bool> RemoveSubOperationAsync(string parentOperationId, string subOperationId)
    {
        var subOperation = await _operationalRepository.GetByIdAsync(subOperationId);
        if (subOperation == null || subOperation.ParentOperationId != parentOperationId)
            return false;

        subOperation.ParentOperationId = null;
        subOperation.UpdatedAt = DateTime.UtcNow;

        await _operationalRepository.UpdateAsync(subOperation);
        return true;
    }
}