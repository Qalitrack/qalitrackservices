using AutoMapper;
using SaccoService.Core.DTOs;
using SaccoService.Core.Entities;

namespace SaccoService.Core.Mappings;

public class SaccoProfile : Profile
{
    public SaccoProfile()
    {
        // Sacco mappings
        CreateMap<Sacco, SaccoDto>()
            .ForMember(dest => dest.MemberCount, opt => opt.Ignore()); // Set manually in service
        CreateMap<Sacco, SaccoDetailDto>()
            .ForMember(dest => dest.MemberCount, opt => opt.Ignore())
            .ForMember(dest => dest.Members, opt => opt.MapFrom(src => src.Members))
            .ForMember(dest => dest.Committees, opt => opt.Ignore()) // Set manually
            .ForMember(dest => dest.RecentMeetings, opt => opt.Ignore()) // Set manually
            .ForMember(dest => dest.Financial, opt => opt.Ignore()); // Set manually

        CreateMap<RegisterSaccoRequest, Sacco>();
        CreateMap<UpdateSaccoRequest, Sacco>();

        // SaccoMember mappings
        CreateMap<SaccoMember, SaccoMemberDto>();
        CreateMap<SaccoMember, SaccoMemberDetailDto>()
            .ForMember(dest => dest.Shares, opt => opt.Ignore()) // Set manually
            .ForMember(dest => dest.Loans, opt => opt.Ignore()) // Set manually
            .ForMember(dest => dest.Memberships, opt => opt.MapFrom(src => src.Memberships));

        CreateMap<AddMemberRequest, SaccoMember>();
        CreateMap<UpdateMemberRequest, SaccoMember>();

        // SaccoMembership mappings
        CreateMap<SaccoMembership, SaccoMembershipDto>();

        // SaccoCommittee mappings
        CreateMap<SaccoCommittee, SaccoCommitteeDto>()
            .ForMember(dest => dest.MemberName, opt => opt.Ignore()); // Set manually

        CreateMap<AddCommitteeMemberRequest, SaccoCommittee>();

        // SaccoMeeting mappings
        CreateMap<SaccoMeeting, SaccoMeetingDto>()
            .ForMember(dest => dest.ChairpersonName, opt => opt.Ignore()) // Set manually
            .ForMember(dest => dest.SecretaryName, opt => opt.Ignore()); // Set manually

        CreateMap<ScheduleMeetingRequest, SaccoMeeting>();
        CreateMap<UpdateMeetingRequest, SaccoMeeting>();

        // SaccoFinancial mappings
        CreateMap<SaccoFinancial, SaccoFinancialDto>();
        CreateMap<UpdateFinancialRequest, SaccoFinancial>();

        // SaccoShare mappings
        CreateMap<SaccoShare, SaccoShareDto>()
            .ForMember(dest => dest.MemberName, opt => opt.Ignore()); // Set manually

        // SaccoLoan mappings
        CreateMap<SaccoLoan, SaccoLoanDto>()
            .ForMember(dest => dest.MemberName, opt => opt.Ignore()) // Set manually
            .ForMember(dest => dest.ApprovedByName, opt => opt.Ignore()); // Set manually
    }
}