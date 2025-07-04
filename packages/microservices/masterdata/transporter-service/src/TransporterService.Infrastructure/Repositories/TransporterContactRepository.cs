using Microsoft.EntityFrameworkCore;
using TransporterService.Core.Entities;
using TransporterService.Core.Interfaces;
using TransporterService.Infrastructure.Data;

namespace TransporterService.Infrastructure.Repositories;

public class TransporterContactRepository : Repository<TransporterContact>, ITransporterContactRepository
{
    public TransporterContactRepository(TransporterDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TransporterContact>> GetContactsByTransporterIdAsync(string transporterId)
    {
        return await _dbSet
            .Where(c => c.TransporterId == transporterId && !c.IsDeleted)
            .OrderBy(c => c.ContactType)
            .ThenBy(c => c.FirstName)
            .ToListAsync();
    }

    public async Task<TransporterContact?> GetPrimaryContactAsync(string transporterId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.TransporterId == transporterId && c.IsPrimary && c.IsActive && !c.IsDeleted);
    }

    public async Task<IEnumerable<TransporterContact>> GetContactsByTypeAsync(string transporterId, ContactType type)
    {
        return await _dbSet
            .Where(c => c.TransporterId == transporterId && c.ContactType == type && !c.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransporterContact>> GetActiveContactsAsync(string transporterId)
    {
        return await _dbSet
            .Where(c => c.TransporterId == transporterId && c.IsActive && !c.IsDeleted)
            .ToListAsync();
    }
}