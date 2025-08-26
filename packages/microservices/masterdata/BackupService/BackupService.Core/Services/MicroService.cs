using BackupService.Core.Dtos;
using BackupService.Core.Entities;
using BackupService.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace BackupService.Core.Services;

public class MicroService : IMicroService
{
    private readonly IMicroserviceRepository _repository;
    private readonly ILogger<MicroService> _logger;

    public MicroService(IMicroserviceRepository repository, ILogger<MicroService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Microservice> CreateMicroserviceAsync(MicroserviceRequest request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating microservice {Name}", request.Name);
            
            // Add validation logic here if needed
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Microservice name is required", nameof(request.Name));
            
            if (string.IsNullOrWhiteSpace(request.ConnectionString))
                throw new ArgumentException("Connection string is required", nameof(request.ConnectionString));

            await _repository.AddMicroserviceAsync(request, ct);
            
            // Retrieve the created microservice to return it
            var createdMicroservice = await _repository.GetMicroserviceAsync(request.Name, ct);
            return createdMicroservice ?? throw new InvalidOperationException("Failed to retrieve created microservice");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating microservice {Name}", request.Name);
            throw;
        }
    }

    public async Task<Microservice> UpdateMicroserviceAsync(string name, MicroserviceRequest request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating microservice {Name}", name);
            
            // Add validation logic here if needed
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Microservice name is required", nameof(request.Name));
            
            if (string.IsNullOrWhiteSpace(request.ConnectionString))
                throw new ArgumentException("Connection string is required", nameof(request.ConnectionString));

            await _repository.UpdateMicroserviceAsync(name, request, ct);
            
            // Retrieve the updated microservice to return it
            var updatedMicroservice = await _repository.GetMicroserviceAsync(request.Name, ct);
            return updatedMicroservice ?? throw new InvalidOperationException("Failed to retrieve updated microservice");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating microservice {Name}", name);
            throw;
        }
    }

    public async Task<Microservice> GetMicroserviceAsync(string name, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting microservice {Name}", name);
            
            var microservice = await _repository.GetMicroserviceAsync(name, ct);
            if (microservice == null)
                throw new KeyNotFoundException($"Microservice {name} not found");
                
            return microservice;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting microservice {Name}", name);
            throw;
        }
    }

    public async Task<List<Microservice>> GetAllMicroservicesAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting all microservices");
            return await _repository.GetAllMicroservicesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all microservices");
            throw;
        }
    }

    public async Task DeleteMicroserviceAsync(string name, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deleting microservice {Name}", name);
            await _repository.DeleteMicroserviceAsync(name, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting microservice {Name}", name);
            throw;
        }
    }
}