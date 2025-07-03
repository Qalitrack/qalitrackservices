using AutoMapper;
using SaccoService.Core.DTOs;
using SaccoService.Core.Entities;
using SaccoService.Core.Interfaces;

namespace SaccoService.Core.Services;

public class SaccoService : ISaccoService
{
    private readonly ISaccoRepository _saccoRepository;
    private readonly ISaccoMemberRepository _memberRepository;
    private readonly ISaccoCommitteeRepository _committeeRepository;
    private readonly ISaccoMeetingRepository _meetingRepository;
    private readonly ISaccoFinancialRepository _financialRepository;
    private readonly ISaccoShareRepository _shareRepository;
    private readonly ISaccoLoanRepository _loanRepository;
    private readonly IMapper _mapper;

    public SaccoService(
        ISaccoRepository saccoRepository,
        ISaccoMemberRepository memberRepository,
        ISaccoCommitteeRepository committeeRepository,
        ISaccoMeetingRepository meetingRepository,
        ISaccoFinancialRepository financialRepository,
        ISaccoShareRepository shareRepository,
        ISaccoLoanRepository loanRepository,
        IMapper mapper)
    {
        _saccoRepository = saccoRepository;
        _memberRepository = memberRepository;
        _committeeRepository = committeeRepository;
        _meetingRepository = meetingRepository;
        _financialRepository = financialRepository;
        _shareRepository = shareRepository;
        _loanRepository = loanRepository;
        _mapper = mapper;
    }

    #region Sacco Management

    public async Task<SaccoDto> RegisterSaccoAsync(RegisterSaccoRequest request)
    {
        // Check if registration number already exists
        if (await _saccoRepository.ExistsByRegistrationNumberAsync(request.RegistrationNumber))
        {
            throw new InvalidOperationException($"A SACCO with registration number {request.RegistrationNumber} already exists");
        }

        // Check if name already exists
        if (await _saccoRepository.ExistsByNameAsync(request.Name))
        {
            throw new InvalidOperationException($"A SACCO with name {request.Name} already exists");
        }

        var sacco = new Sacco
        {
            Name = request.Name,
            RegistrationNumber = request.RegistrationNumber,
            LicenseNumber = request.LicenseNumber,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            Address = request.Address,
            EstablishedDate = request.EstablishedDate,
            SaccoType = request.SaccoType,
            MembershipCapacity = request.MembershipCapacity,
            Description = request.Description,
            Website = request.Website,
            ShareCapitalMinimum = request.ShareCapitalMinimum,
            ShareValue = request.ShareValue,
            Status = SaccoStatus.Active
        };

        await _saccoRepository.AddAsync(sacco);
        return _mapper.Map<SaccoDto>(sacco);
    }

    public async Task<SaccoDto?> GetSaccoAsync(string id)
    {
        var sacco = await _saccoRepository.GetByIdAsync(id);
        if (sacco == null) return null;

        var saccoDto = _mapper.Map<SaccoDto>(sacco);
        saccoDto.MemberCount = await _memberRepository.GetMemberCountBySaccoAsync(id);
        return saccoDto;
    }

    public async Task<SaccoDetailDto?> GetSaccoDetailsAsync(string id)
    {
        var sacco = await _saccoRepository.GetSaccoWithMembersAsync(id);
        if (sacco == null) return null;

        var saccoDetailDto = _mapper.Map<SaccoDetailDto>(sacco);
        
        // Get additional details
        saccoDetailDto.Committees = _mapper.Map<List<SaccoCommitteeDto>>(
            await _committeeRepository.GetActiveCommitteeMembersAsync(id));
        
        saccoDetailDto.RecentMeetings = _mapper.Map<List<SaccoMeetingDto>>(
            (await _meetingRepository.GetMeetingsBySaccoAsync(id)).Take(5));
        
        saccoDetailDto.Financial = _mapper.Map<SaccoFinancialDto>(
            await _financialRepository.GetBySaccoIdAsync(id));

        return saccoDetailDto;
    }

    public async Task<IEnumerable<SaccoDto>> GetSaccosAsync()
    {
        var saccos = await _saccoRepository.GetAllAsync();
        var saccoDtos = new List<SaccoDto>();

        foreach (var sacco in saccos)
        {
            var dto = _mapper.Map<SaccoDto>(sacco);
            dto.MemberCount = await _memberRepository.GetMemberCountBySaccoAsync(sacco.Id);
            saccoDtos.Add(dto);
        }

        return saccoDtos;
    }

    public async Task<IEnumerable<SaccoDto>> GetSaccosPagedAsync(int page, int pageSize)
    {
        var saccos = await _saccoRepository.GetPagedAsync(page, pageSize);
        var saccoDtos = new List<SaccoDto>();

        foreach (var sacco in saccos)
        {
            var dto = _mapper.Map<SaccoDto>(sacco);
            dto.MemberCount = await _memberRepository.GetMemberCountBySaccoAsync(sacco.Id);
            saccoDtos.Add(dto);
        }

        return saccoDtos;
    }

    public async Task<IEnumerable<SaccoDto>> SearchSaccosAsync(string searchTerm)
    {
        var saccos = await _saccoRepository.SearchSaccosAsync(searchTerm);
        var saccoDtos = new List<SaccoDto>();

        foreach (var sacco in saccos)
        {
            var dto = _mapper.Map<SaccoDto>(sacco);
            dto.MemberCount = await _memberRepository.GetMemberCountBySaccoAsync(sacco.Id);
            saccoDtos.Add(dto);
        }

        return saccoDtos;
    }

    public async Task<SaccoDto> UpdateSaccoAsync(string id, UpdateSaccoRequest request)
    {
        var sacco = await _saccoRepository.GetByIdAsync(id);
        if (sacco == null)
        {
            throw new ArgumentException($"SACCO with ID {id} not found");
        }

        // Update properties
        sacco.Name = request.Name;
        sacco.LicenseNumber = request.LicenseNumber;
        sacco.ContactEmail = request.ContactEmail;
        sacco.ContactPhone = request.ContactPhone;
        sacco.Address = request.Address;
        sacco.SaccoType = request.SaccoType;
        sacco.MembershipCapacity = request.MembershipCapacity;
        sacco.Status = request.Status;
        sacco.Description = request.Description;
        sacco.Website = request.Website;
        sacco.ShareCapitalMinimum = request.ShareCapitalMinimum;
        sacco.ShareValue = request.ShareValue;
        sacco.UpdatedAt = DateTime.UtcNow;

        await _saccoRepository.UpdateAsync(sacco);
        
        var dto = _mapper.Map<SaccoDto>(sacco);
        dto.MemberCount = await _memberRepository.GetMemberCountBySaccoAsync(id);
        return dto;
    }

    public async Task<bool> DeleteSaccoAsync(string id)
    {
        var sacco = await _saccoRepository.GetByIdAsync(id);
        if (sacco == null) return false;

        await _saccoRepository.DeleteAsync(sacco);
        return true;
    }

    public async Task<bool> ActivateSaccoAsync(string id)
    {
        var sacco = await _saccoRepository.GetByIdAsync(id);
        if (sacco == null) return false;

        sacco.Status = SaccoStatus.Active;
        sacco.UpdatedAt = DateTime.UtcNow;
        await _saccoRepository.UpdateAsync(sacco);
        return true;
    }

    public async Task<bool> DeactivateSaccoAsync(string id)
    {
        var sacco = await _saccoRepository.GetByIdAsync(id);
        if (sacco == null) return false;

        sacco.Status = SaccoStatus.Inactive;
        sacco.UpdatedAt = DateTime.UtcNow;
        await _saccoRepository.UpdateAsync(sacco);
        return true;
    }

    #endregion

    #region Member Management

    public async Task<SaccoMemberDto> AddMemberAsync(string saccoId, AddMemberRequest request)
    {
        var sacco = await _saccoRepository.GetByIdAsync(saccoId);
        if (sacco == null)
        {
            throw new ArgumentException($"SACCO with ID {saccoId} not found");
        }

        // Check membership capacity
        var currentMemberCount = await _memberRepository.GetMemberCountBySaccoAsync(saccoId);
        if (currentMemberCount >= sacco.MembershipCapacity)
        {
            throw new InvalidOperationException("SACCO has reached maximum membership capacity");
        }

        // Check if ID number already exists
        if (await _memberRepository.ExistsByIdNumberAsync(request.IdNumber))
        {
            throw new InvalidOperationException($"A member with ID number {request.IdNumber} already exists");
        }

        var member = new SaccoMember
        {
            SaccoId = saccoId,
            MemberNumber = await GenerateMemberNumberAsync(saccoId),
            Name = request.Name,
            IdNumber = request.IdNumber,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            Address = request.Address,
            JoinDate = DateTime.UtcNow,
            MembershipType = request.MembershipType,
            DateOfBirth = request.DateOfBirth,
            Occupation = request.Occupation,
            EmergencyContact = request.EmergencyContact,
            EmergencyPhone = request.EmergencyPhone,
            Status = MembershipStatus.Active
        };

        await _memberRepository.AddAsync(member);
        return _mapper.Map<SaccoMemberDto>(member);
    }

    public async Task<IEnumerable<SaccoMemberDto>> GetSaccoMembersAsync(string saccoId)
    {
        var members = await _memberRepository.GetMembersBySaccoAsync(saccoId);
        return _mapper.Map<IEnumerable<SaccoMemberDto>>(members);
    }

    public async Task<SaccoMemberDto?> GetMemberAsync(string memberId)
    {
        var member = await _memberRepository.GetByIdAsync(memberId);
        return member == null ? null : _mapper.Map<SaccoMemberDto>(member);
    }

    public async Task<SaccoMemberDetailDto?> GetMemberDetailsAsync(string memberId)
    {
        var member = await _memberRepository.GetMemberWithDetailsAsync(memberId);
        if (member == null) return null;

        var memberDetailDto = _mapper.Map<SaccoMemberDetailDto>(member);
        
        // Get additional details
        memberDetailDto.Shares = _mapper.Map<List<SaccoShareDto>>(
            await _shareRepository.GetSharesByMemberAsync(memberId));
        
        memberDetailDto.Loans = _mapper.Map<List<SaccoLoanDto>>(
            await _loanRepository.GetLoansByMemberAsync(memberId));

        return memberDetailDto;
    }

    public async Task<SaccoMemberDto> UpdateMemberAsync(string memberId, UpdateMemberRequest request)
    {
        var member = await _memberRepository.GetByIdAsync(memberId);
        if (member == null)
        {
            throw new ArgumentException($"Member with ID {memberId} not found");
        }

        // Update properties
        member.Name = request.Name;
        member.ContactEmail = request.ContactEmail;
        member.ContactPhone = request.ContactPhone;
        member.Address = request.Address;
        member.MembershipType = request.MembershipType;
        member.Status = request.Status;
        member.Occupation = request.Occupation;
        member.EmergencyContact = request.EmergencyContact;
        member.EmergencyPhone = request.EmergencyPhone;
        member.UpdatedAt = DateTime.UtcNow;

        await _memberRepository.UpdateAsync(member);
        return _mapper.Map<SaccoMemberDto>(member);
    }

    public async Task<bool> RemoveMemberAsync(string memberId)
    {
        var member = await _memberRepository.GetByIdAsync(memberId);
        if (member == null) return false;

        await _memberRepository.DeleteAsync(member);
        return true;
    }

    public async Task<IEnumerable<SaccoMemberDto>> SearchMembersAsync(string saccoId, string searchTerm)
    {
        var members = await _memberRepository.SearchMembersAsync(saccoId, searchTerm);
        return _mapper.Map<IEnumerable<SaccoMemberDto>>(members);
    }

    #endregion

    #region Committee Management

    public async Task<SaccoCommitteeDto> AddCommitteeMemberAsync(string saccoId, AddCommitteeMemberRequest request)
    {
        var sacco = await _saccoRepository.GetByIdAsync(saccoId);
        if (sacco == null)
        {
            throw new ArgumentException($"SACCO with ID {saccoId} not found");
        }

        var member = await _memberRepository.GetByIdAsync(request.MemberId);
        if (member == null || member.SaccoId != saccoId)
        {
            throw new ArgumentException($"Member with ID {request.MemberId} not found in this SACCO");
        }

        // Check if position already exists
        if (await _committeeRepository.ExistsByPositionAsync(saccoId, request.Position))
        {
            throw new InvalidOperationException($"Position {request.Position} is already occupied");
        }

        var committeeMemb = new SaccoCommittee
        {
            SaccoId = saccoId,
            MemberId = request.MemberId,
            Position = request.Position,
            AppointmentDate = DateTime.UtcNow,
            TermEndDate = request.TermEndDate,
            Responsibilities = request.Responsibilities,
            IsElected = request.IsElected,
            Allowance = request.Allowance,
            Status = CommitteeStatus.Active
        };

        await _committeeRepository.AddAsync(committeeMemb);
        
        var dto = _mapper.Map<SaccoCommitteeDto>(committeeMemb);
        dto.MemberName = member.Name;
        return dto;
    }

    public async Task<IEnumerable<SaccoCommitteeDto>> GetCommitteeMembersAsync(string saccoId)
    {
        var committeeMembers = await _committeeRepository.GetActiveCommitteeMembersAsync(saccoId);
        var dtos = new List<SaccoCommitteeDto>();

        foreach (var cm in committeeMembers)
        {
            var member = await _memberRepository.GetByIdAsync(cm.MemberId);
            var dto = _mapper.Map<SaccoCommitteeDto>(cm);
            dto.MemberName = member?.Name ?? "Unknown";
            dtos.Add(dto);
        }

        return dtos;
    }

    public async Task<bool> RemoveCommitteeMemberAsync(string committeeId)
    {
        var committee = await _committeeRepository.GetByIdAsync(committeeId);
        if (committee == null) return false;

        await _committeeRepository.DeleteAsync(committee);
        return true;
    }

    #endregion

    #region Meeting Management

    public async Task<SaccoMeetingDto> ScheduleMeetingAsync(string saccoId, ScheduleMeetingRequest request)
    {
        var sacco = await _saccoRepository.GetByIdAsync(saccoId);
        if (sacco == null)
        {
            throw new ArgumentException($"SACCO with ID {saccoId} not found");
        }

        var meeting = new SaccoMeeting
        {
            SaccoId = saccoId,
            Title = request.Title,
            Description = request.Description,
            ScheduledDate = request.ScheduledDate,
            Location = request.Location,
            MeetingType = request.MeetingType,
            Agenda = request.Agenda,
            QuorumRequired = request.QuorumRequired,
            ChairpersonId = request.ChairpersonId,
            SecretaryId = request.SecretaryId,
            Status = MeetingStatus.Scheduled
        };

        await _meetingRepository.AddAsync(meeting);
        
        var dto = _mapper.Map<SaccoMeetingDto>(meeting);
        
        // Set names for chairperson and secretary if provided
        if (!string.IsNullOrEmpty(request.ChairpersonId))
        {
            var chairperson = await _memberRepository.GetByIdAsync(request.ChairpersonId);
            dto.ChairpersonName = chairperson?.Name;
        }
        
        if (!string.IsNullOrEmpty(request.SecretaryId))
        {
            var secretary = await _memberRepository.GetByIdAsync(request.SecretaryId);
            dto.SecretaryName = secretary?.Name;
        }

        return dto;
    }

    public async Task<IEnumerable<SaccoMeetingDto>> GetMeetingsAsync(string saccoId)
    {
        var meetings = await _meetingRepository.GetMeetingsBySaccoAsync(saccoId);
        var dtos = new List<SaccoMeetingDto>();

        foreach (var meeting in meetings)
        {
            var dto = _mapper.Map<SaccoMeetingDto>(meeting);
            
            if (!string.IsNullOrEmpty(meeting.ChairpersonId))
            {
                var chairperson = await _memberRepository.GetByIdAsync(meeting.ChairpersonId);
                dto.ChairpersonName = chairperson?.Name;
            }
            
            if (!string.IsNullOrEmpty(meeting.SecretaryId))
            {
                var secretary = await _memberRepository.GetByIdAsync(meeting.SecretaryId);
                dto.SecretaryName = secretary?.Name;
            }

            dtos.Add(dto);
        }

        return dtos;
    }

    public async Task<IEnumerable<SaccoMeetingDto>> GetUpcomingMeetingsAsync(string saccoId)
    {
        var meetings = await _meetingRepository.GetUpcomingMeetingsAsync(saccoId);
        return _mapper.Map<IEnumerable<SaccoMeetingDto>>(meetings);
    }

    public async Task<SaccoMeetingDto> UpdateMeetingAsync(string meetingId, UpdateMeetingRequest request)
    {
        var meeting = await _meetingRepository.GetByIdAsync(meetingId);
        if (meeting == null)
        {
            throw new ArgumentException($"Meeting with ID {meetingId} not found");
        }

        // Update properties
        meeting.Title = request.Title;
        meeting.Description = request.Description;
        meeting.ScheduledDate = request.ScheduledDate;
        meeting.Location = request.Location;
        meeting.Status = request.Status;
        meeting.Agenda = request.Agenda;
        meeting.Minutes = request.Minutes;
        meeting.AttendanceCount = request.AttendanceCount;
        meeting.QuorumMet = request.QuorumMet;
        meeting.UpdatedAt = DateTime.UtcNow;

        await _meetingRepository.UpdateAsync(meeting);
        return _mapper.Map<SaccoMeetingDto>(meeting);
    }

    public async Task<bool> CancelMeetingAsync(string meetingId)
    {
        var meeting = await _meetingRepository.GetByIdAsync(meetingId);
        if (meeting == null) return false;

        meeting.Status = MeetingStatus.Cancelled;
        meeting.UpdatedAt = DateTime.UtcNow;
        await _meetingRepository.UpdateAsync(meeting);
        return true;
    }

    #endregion

    #region Financial Management

    public async Task<SaccoFinancialDto?> GetFinancialInformationAsync(string saccoId)
    {
        var financial = await _financialRepository.GetBySaccoIdAsync(saccoId);
        return financial == null ? null : _mapper.Map<SaccoFinancialDto>(financial);
    }

    public async Task<SaccoFinancialDto> UpdateFinancialInformationAsync(string saccoId, UpdateFinancialRequest request)
    {
        var sacco = await _saccoRepository.GetByIdAsync(saccoId);
        if (sacco == null)
        {
            throw new ArgumentException($"SACCO with ID {saccoId} not found");
        }

        var financial = await _financialRepository.GetBySaccoIdAsync(saccoId);
        
        if (financial == null)
        {
            // Create new financial record
            financial = new SaccoFinancial
            {
                SaccoId = saccoId,
                FinancialYearStart = DateTime.UtcNow.AddYears(-1),
                FinancialYearEnd = DateTime.UtcNow
            };
        }

        // Update financial information
        financial.TotalAssets = request.TotalAssets;
        financial.TotalLiabilities = request.TotalLiabilities;
        financial.ShareCapital = request.ShareCapital;
        financial.ReservesFunds = request.ReservesFunds;
        financial.TotalSavings = request.TotalSavings;
        financial.TotalLoansOutstanding = request.TotalLoansOutstanding;
        financial.CashAtBank = request.CashAtBank;
        financial.CashAtHand = request.CashAtHand;
        financial.AnnualIncome = request.AnnualIncome;
        financial.AnnualExpenses = request.AnnualExpenses;
        financial.NetSurplus = request.AnnualIncome - request.AnnualExpenses;
        financial.InterestRateOnLoans = request.InterestRateOnLoans;
        financial.InterestRateOnSavings = request.InterestRateOnSavings;
        financial.UpdatedAt = DateTime.UtcNow;

        if (string.IsNullOrEmpty(financial.Id))
        {
            await _financialRepository.AddAsync(financial);
        }
        else
        {
            await _financialRepository.UpdateAsync(financial);
        }

        return _mapper.Map<SaccoFinancialDto>(financial);
    }

    #endregion

    #region Share Management

    public async Task<IEnumerable<SaccoShareDto>> GetSharesAsync(string saccoId)
    {
        var shares = await _shareRepository.GetSharesBySaccoAsync(saccoId);
        var dtos = new List<SaccoShareDto>();

        foreach (var share in shares)
        {
            var member = await _memberRepository.GetByIdAsync(share.MemberId);
            var dto = _mapper.Map<SaccoShareDto>(share);
            dto.MemberName = member?.Name ?? "Unknown";
            dtos.Add(dto);
        }

        return dtos;
    }

    public async Task<IEnumerable<SaccoShareDto>> GetMemberSharesAsync(string memberId)
    {
        var shares = await _shareRepository.GetSharesByMemberAsync(memberId);
        var member = await _memberRepository.GetByIdAsync(memberId);
        var dtos = new List<SaccoShareDto>();

        foreach (var share in shares)
        {
            var dto = _mapper.Map<SaccoShareDto>(share);
            dto.MemberName = member?.Name ?? "Unknown";
            dtos.Add(dto);
        }

        return dtos;
    }

    #endregion

    #region Loan Management

    public async Task<IEnumerable<SaccoLoanDto>> GetLoansAsync(string saccoId)
    {
        var loans = await _loanRepository.GetLoansBySaccoAsync(saccoId);
        var dtos = new List<SaccoLoanDto>();

        foreach (var loan in loans)
        {
            var member = await _memberRepository.GetByIdAsync(loan.MemberId);
            var dto = _mapper.Map<SaccoLoanDto>(loan);
            dto.MemberName = member?.Name ?? "Unknown";
            
            if (!string.IsNullOrEmpty(loan.ApprovedById))
            {
                var approver = await _memberRepository.GetByIdAsync(loan.ApprovedById);
                dto.ApprovedByName = approver?.Name;
            }
            
            dtos.Add(dto);
        }

        return dtos;
    }

    public async Task<IEnumerable<SaccoLoanDto>> GetMemberLoansAsync(string memberId)
    {
        var loans = await _loanRepository.GetLoansByMemberAsync(memberId);
        var member = await _memberRepository.GetByIdAsync(memberId);
        var dtos = new List<SaccoLoanDto>();

        foreach (var loan in loans)
        {
            var dto = _mapper.Map<SaccoLoanDto>(loan);
            dto.MemberName = member?.Name ?? "Unknown";
            
            if (!string.IsNullOrEmpty(loan.ApprovedById))
            {
                var approver = await _memberRepository.GetByIdAsync(loan.ApprovedById);
                dto.ApprovedByName = approver?.Name;
            }
            
            dtos.Add(dto);
        }

        return dtos;
    }

    #endregion

    #region Helper Methods

    private async Task<string> GenerateMemberNumberAsync(string saccoId)
    {
        var memberCount = await _memberRepository.GetMemberCountBySaccoAsync(saccoId);
        var sacco = await _saccoRepository.GetByIdAsync(saccoId);
        var prefix = sacco?.Name.Substring(0, Math.Min(3, sacco.Name.Length)).ToUpper() ?? "SAC";
        return $"{prefix}{(memberCount + 1):D4}";
    }

    #endregion
}