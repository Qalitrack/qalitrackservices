using SaccoService.Core.Entities;

namespace SaccoService.Core.Interfaces;

public interface ISaccoLoanRepository : IRepository<SaccoLoan>
{
    Task<IEnumerable<SaccoLoan>> GetLoansBySaccoAsync(string saccoId);
    Task<IEnumerable<SaccoLoan>> GetLoansByMemberAsync(string memberId);
    Task<IEnumerable<SaccoLoan>> GetLoansByStatusAsync(string saccoId, LoanStatus status);
    Task<decimal> GetTotalOutstandingBySaccoAsync(string saccoId);
    Task<decimal> GetTotalOutstandingByMemberAsync(string memberId);
    Task<bool> ExistsByLoanNumberAsync(string loanNumber);
    Task<SaccoLoan?> GetByLoanNumberAsync(string loanNumber);
}