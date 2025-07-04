using SaccoService.Core.Entities;

namespace SaccoService.Core.Interfaces;

public interface ISaccoShareRepository : IRepository<SaccoShare>
{
    Task<IEnumerable<SaccoShare>> GetSharesBySaccoAsync(string saccoId);
    Task<IEnumerable<SaccoShare>> GetSharesByMemberAsync(string memberId);
    Task<decimal> GetTotalSharesBySaccoAsync(string saccoId);
    Task<decimal> GetTotalSharesByMemberAsync(string memberId);
    Task<bool> ExistsByCertificateNumberAsync(string certificateNumber);
}