using SaccoService.Core.Entities;

namespace SaccoService.Core.Interfaces;

public interface ISaccoRepository : IRepository<Sacco>
{
    Task<IEnumerable<Sacco>> SearchSaccosAsync(string searchTerm);
    Task<bool> ExistsByRegistrationNumberAsync(string registrationNumber);
    Task<bool> ExistsByNameAsync(string name);
    Task<Sacco?> GetSaccoWithMembersAsync(string id);
    Task<Sacco?> GetSaccoWithFinancialAsync(string id);
    Task<IEnumerable<Sacco>> GetSaccosByTypeAsync(SaccoType type);
    Task<IEnumerable<Sacco>> GetSaccosByStatusAsync(SaccoStatus status);
}