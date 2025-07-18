using TransporterService.Core.Entities;

namespace TransporterService.Core.Interfaces;

public interface ITransporterRepository : IRepository<Transporter>
{
    Task<IEnumerable<Transporter>> GetActiveTransportersAsync();
    Task<IEnumerable<Transporter>> GetTransportersByTypeAsync(TransporterType type);
    Task<IEnumerable<Transporter>> GetTransportersByStatusAsync(TransporterStatus status);
    Task<Transporter?> GetByRegistrationNumberAsync(string registrationNumber);
    Task<IEnumerable<Transporter>> GetAvailableTransportersAsync(DateTime date, string? routeId = null);
    Task<IEnumerable<Transporter>> SearchTransportersAsync(string searchTerm);
    Task<bool> IsRegistrationNumberUniqueAsync(string registrationNumber, string? excludeId = null);
    Task<IEnumerable<Transporter>> GetDualRoleTransportersAsync();
}