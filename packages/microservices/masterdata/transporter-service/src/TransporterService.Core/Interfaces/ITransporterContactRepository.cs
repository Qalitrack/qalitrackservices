using TransporterService.Core.Entities;

namespace TransporterService.Core.Interfaces;

public interface ITransporterContactRepository : IRepository<TransporterContact>
{
    Task<IEnumerable<TransporterContact>> GetContactsByTransporterIdAsync(string transporterId);
    Task<TransporterContact?> GetPrimaryContactAsync(string transporterId);
    Task<IEnumerable<TransporterContact>> GetContactsByTypeAsync(string transporterId, ContactType type);
    Task<IEnumerable<TransporterContact>> GetActiveContactsAsync(string transporterId);
}