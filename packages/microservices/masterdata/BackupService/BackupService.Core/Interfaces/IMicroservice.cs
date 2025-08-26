using BackupService.Core.Dtos;
using BackupService.Core.Entities;

namespace BackupService.Core.Interfaces;

public interface IMicroService
{
    Task<Microservice> CreateMicroserviceAsync(MicroserviceRequest request, CancellationToken ct = default);
    Task<Microservice> UpdateMicroserviceAsync(string name, MicroserviceRequest request, CancellationToken ct = default);
    Task<Microservice> GetMicroserviceAsync(string name, CancellationToken ct = default);
    Task<List<Microservice>> GetAllMicroservicesAsync(CancellationToken ct = default);
    Task DeleteMicroserviceAsync(string name, CancellationToken ct = default);
}