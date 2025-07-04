using SaccoService.Core.Entities;

namespace SaccoService.Core.Interfaces;

public interface ISaccoMemberRepository : IRepository<SaccoMember>
{
    Task<IEnumerable<SaccoMember>> GetMembersBySaccoAsync(string saccoId);
    Task<bool> ExistsByIdNumberAsync(string idNumber);
    Task<bool> ExistsByMemberNumberAsync(string memberNumber);
    Task<SaccoMember?> GetByMemberNumberAsync(string memberNumber);
    Task<IEnumerable<SaccoMember>> SearchMembersAsync(string saccoId, string searchTerm);
    Task<int> GetMemberCountBySaccoAsync(string saccoId);
    Task<SaccoMember?> GetMemberWithDetailsAsync(string memberId);
}