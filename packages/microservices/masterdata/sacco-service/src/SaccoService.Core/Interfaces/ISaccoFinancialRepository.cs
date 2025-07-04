using SaccoService.Core.Entities;

namespace SaccoService.Core.Interfaces;

public interface ISaccoFinancialRepository : IRepository<SaccoFinancial>
{
    Task<SaccoFinancial?> GetBySaccoIdAsync(string saccoId);
    Task<IEnumerable<SaccoFinancial>> GetByFinancialYearAsync(DateTime startDate, DateTime endDate);
}