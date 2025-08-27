using BackupService.Core.Dtos;
using BackupService.Core.Entities;
using BackupService.Core.Interfaces;
using BackupService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackupService.Infrastructure.Repositories;

public class MicroserviceRepository : IMicroserviceRepository
{
    private readonly BackupServiceDbContext _context;
    private readonly ILogger _logger;

    public MicroserviceRepository(BackupServiceDbContext context, ILogger<MicroserviceRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task AddMicroserviceAsync(MicroserviceRequest request, CancellationToken ct = default)
    {
        var existing = await _context.Microservices
            .FirstOrDefaultAsync(m => m.Name == request.Name, ct);
        if (existing != null)
        {
            _logger.LogWarning("Microservice {Name} already exists", request.Name);
            throw new InvalidOperationException($"Microservice {request.Name} already exists");
        }

        var microservice = new Microservice
        {
            Name = request.Name,
            ConnectionString = request.ConnectionString, // Assume encrypted by caller
            Status = request.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Microservices.Add(microservice);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Added microservice {Name}", request.Name);
    }
    

    public async Task UpdateMicroserviceAsync(string name, MicroserviceRequest request, CancellationToken ct = default)
    {
        var microservice = await _context.Microservices
                               .FirstOrDefaultAsync(m => m.Name == name, ct)
                           ?? throw new KeyNotFoundException($"Microservice {name} not found");

        if (name != request.Name)
        {
            var nameExists = await _context.Microservices
                .AnyAsync(m => m.Name == request.Name, ct);
            if (nameExists)
                throw new InvalidOperationException($"Microservice name {request.Name} already exists");
            microservice.Name = request.Name;
        }

        microservice.ConnectionString = request.ConnectionString;
        microservice.Status = request.Status;
        microservice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Updated microservice {Name}", name);
    }

    public async Task<Microservice?> GetMicroserviceAsync(string name, CancellationToken ct = default)
    {
        var microservice = await _context.Microservices
            .FirstOrDefaultAsync(m => m.Name == name, ct);
        if (microservice == null)
            _logger.LogWarning("Microservice {Name} not found", name);
        return microservice;
    }

    public async Task<List<Microservice>> GetAllMicroservicesAsync(CancellationToken ct = default)
    {
        return await _context.Microservices
            .ToListAsync(ct);
    }

    public async Task DeleteMicroserviceAsync(string name, CancellationToken ct = default)
    {
        var microservice = await _context.Microservices
                               .FirstOrDefaultAsync(m => m.Name == name, ct)
                           ?? throw new KeyNotFoundException($"Microservice {name} not found");

        _context.Microservices.Remove(microservice);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Deleted microservice {Name}", name);
    }

}