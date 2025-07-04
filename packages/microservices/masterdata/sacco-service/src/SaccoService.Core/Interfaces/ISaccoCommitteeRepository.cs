using SaccoService.Core.Entities;

namespace SaccoService.Core.Interfaces;

public interface ISaccoCommitteeRepository : IRepository<SaccoCommittee>
{
    Task<IEnumerable<SaccoCommittee>> GetCommitteeBySaccoAsync(string saccoId);
    Task<bool> ExistsByPositionAsync(string saccoId, CommitteePosition position);
    Task<SaccoCommittee?> GetByPositionAsync(string saccoId, CommitteePosition position);
    Task<IEnumerable<SaccoCommittee>> GetActiveCommitteeMembersAsync(string saccoId);
}