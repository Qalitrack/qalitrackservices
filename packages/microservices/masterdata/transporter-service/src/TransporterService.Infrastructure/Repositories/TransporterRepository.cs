using Microsoft.EntityFrameworkCore;
using TransporterService.Core.Entities;
using TransporterService.Core.Interfaces;
using TransporterService.Infrastructure.Data;

namespace TransporterService.Infrastructure.Repositories;

public class TransporterRepository : Repository<Transporter>, ITransporterRepository
{
    public TransporterRepository(TransporterDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Transporter>> GetActiveTransportersAsync()
    {
        return await _dbSet
            .Where(t => t.Status == TransporterStatus.Active && !t.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transporter>> GetTransportersByTypeAsync(TransporterType type)
    {
        return await _dbSet
            .Where(t => t.TransporterType == type && !t.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transporter>> GetTransportersByStatusAsync(TransporterStatus status)
    {
        return await _dbSet
            .Where(t => t.Status == status && !t.IsDeleted)
            .ToListAsync();
    }

    public async Task<Transporter?> GetByRegistrationNumberAsync(string registrationNumber)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => t.RegistrationNumber == registrationNumber && !t.IsDeleted);
    }

    public async Task<IEnumerable<Transporter>> GetAvailableTransportersAsync(DateTime date, string? routeId = null)
    {
        var query = _dbSet
            .Include(t => t.Fleet)
            .Where(t => t.Status == TransporterStatus.Active && !t.IsDeleted)
            .Where(t => t.Fleet.Any(v => v.Status == VehicleStatus.Available));

        // Add route filtering logic here if needed
        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Transporter>> SearchTransportersAsync(string searchTerm)
    {
        return await _dbSet
            .Where(t => !t.IsDeleted &&
                (t.Name.Contains(searchTerm) ||
                 t.RegistrationNumber.Contains(searchTerm) ||
                 (t.ContactEmail != null && t.ContactEmail.Contains(searchTerm))))
            .ToListAsync();
    }

    public async Task<bool> IsRegistrationNumberUniqueAsync(string registrationNumber, string? excludeId = null)
    {
        var query = _dbSet.Where(t => t.RegistrationNumber == registrationNumber && !t.IsDeleted);
        
        if (excludeId != null)
        {
            query = query.Where(t => t.Id != excludeId);
        }
        
        return !await query.AnyAsync();
    }

    public async Task<IEnumerable<Transporter>> GetDualRoleTransportersAsync()
    {
        return await _dbSet
            .Where(t => t.IsCustomer && !t.IsDeleted)
            .ToListAsync();
    }
}