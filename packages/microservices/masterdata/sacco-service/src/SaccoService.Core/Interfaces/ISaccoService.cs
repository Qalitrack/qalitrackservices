using SaccoService.Core.DTOs;

namespace SaccoService.Core.Interfaces;

public interface ISaccoService
{
    // Sacco Management
    Task<SaccoDto> RegisterSaccoAsync(RegisterSaccoRequest request);
    Task<SaccoDto?> GetSaccoAsync(string id);
    Task<SaccoDetailDto?> GetSaccoDetailsAsync(string id);
    Task<IEnumerable<SaccoDto>> GetSaccosAsync();
    Task<IEnumerable<SaccoDto>> GetSaccosPagedAsync(int page, int pageSize);
    Task<IEnumerable<SaccoDto>> SearchSaccosAsync(string searchTerm);
    Task<SaccoDto> UpdateSaccoAsync(string id, UpdateSaccoRequest request);
    Task<bool> DeleteSaccoAsync(string id);
    Task<bool> ActivateSaccoAsync(string id);
    Task<bool> DeactivateSaccoAsync(string id);

    // Member Management
    Task<SaccoMemberDto> AddMemberAsync(string saccoId, AddMemberRequest request);
    Task<IEnumerable<SaccoMemberDto>> GetSaccoMembersAsync(string saccoId);
    Task<SaccoMemberDto?> GetMemberAsync(string memberId);
    Task<SaccoMemberDetailDto?> GetMemberDetailsAsync(string memberId);
    Task<SaccoMemberDto> UpdateMemberAsync(string memberId, UpdateMemberRequest request);
    Task<bool> RemoveMemberAsync(string memberId);
    Task<IEnumerable<SaccoMemberDto>> SearchMembersAsync(string saccoId, string searchTerm);

    // Committee Management
    Task<SaccoCommitteeDto> AddCommitteeMemberAsync(string saccoId, AddCommitteeMemberRequest request);
    Task<IEnumerable<SaccoCommitteeDto>> GetCommitteeMembersAsync(string saccoId);
    Task<bool> RemoveCommitteeMemberAsync(string committeeId);

    // Meeting Management
    Task<SaccoMeetingDto> ScheduleMeetingAsync(string saccoId, ScheduleMeetingRequest request);
    Task<IEnumerable<SaccoMeetingDto>> GetMeetingsAsync(string saccoId);
    Task<IEnumerable<SaccoMeetingDto>> GetUpcomingMeetingsAsync(string saccoId);
    Task<SaccoMeetingDto> UpdateMeetingAsync(string meetingId, UpdateMeetingRequest request);
    Task<bool> CancelMeetingAsync(string meetingId);

    // Financial Management
    Task<SaccoFinancialDto?> GetFinancialInformationAsync(string saccoId);
    Task<SaccoFinancialDto> UpdateFinancialInformationAsync(string saccoId, UpdateFinancialRequest request);

    // Share Management
    Task<IEnumerable<SaccoShareDto>> GetSharesAsync(string saccoId);
    Task<IEnumerable<SaccoShareDto>> GetMemberSharesAsync(string memberId);

    // Loan Management (read-only for this service)
    Task<IEnumerable<SaccoLoanDto>> GetLoansAsync(string saccoId);
    Task<IEnumerable<SaccoLoanDto>> GetMemberLoansAsync(string memberId);
}