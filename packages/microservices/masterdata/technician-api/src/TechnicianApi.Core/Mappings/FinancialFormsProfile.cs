using AutoMapper;
using TechnicianApi.Core.DTOs.PettyCash;
using TechnicianApi.Core.DTOs.AdvanceReturn;
using TechnicianApi.Core.DTOs.PerDiemReturn;
using TechnicianApi.Core.DTOs.Claim;
using TechnicianApi.Core.DTOs.Refund;
using TechnicianApi.Core.DTOs.Balance;
using TechnicianApi.Core.Entities;

namespace TechnicianApi.Core.Mappings;

public class FinancialFormsProfile : Profile
{
    public FinancialFormsProfile()
    {
        // PettyCashAdvanceForm mappings
        CreateMap<PettyCashAdvanceForm, PettyCashAdvanceFormResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<CreatePettyCashAdvanceFormDto, PettyCashAdvanceForm>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => PettyCashStatus.Pending))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Assignment, opt => opt.Ignore());

        CreateMap<UpdatePettyCashAdvanceFormDto, PettyCashAdvanceForm>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // AdvanceReturnForm mappings
        CreateMap<AdvanceReturnLineItem, AdvanceReturnLineItemDto>().ReverseMap();

        CreateMap<AdvanceReturnForm, AdvanceReturnFormResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<CreateAdvanceReturnFormDto, AdvanceReturnForm>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => AdvanceReturnStatus.Pending))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.LineItems.Sum(li => li.AmountInKsh)))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Assignment, opt => opt.Ignore());

        CreateMap<UpdateAdvanceReturnFormDto, AdvanceReturnForm>()
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom((src, dest) =>
                src.LineItems != null ? src.LineItems.Sum(li => li.AmountInKsh) : dest.TotalAmount))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // PerDiemReturnForm mappings
        CreateMap<PerDiemReturnForm, PerDiemReturnFormResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<CreatePerDiemReturnFormDto, PerDiemReturnForm>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => PerDiemReturnStatus.Pending))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src =>
                src.FaresOrCarExpense + src.Mileage + src.Meals + src.Medical + src.Incidentals))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Assignment, opt => opt.Ignore());

        CreateMap<UpdatePerDiemReturnFormDto, PerDiemReturnForm>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Claim mappings
        CreateMap<Claim, ClaimResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<CreateClaimDto, Claim>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ClaimStatus.Pending))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Assignment, opt => opt.Ignore());

        CreateMap<UpdateClaimDto, Claim>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Refund mappings
        CreateMap<Refund, RefundResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<CreateRefundDto, Refund>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => RefundStatus.Pending))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Assignment, opt => opt.Ignore());

        CreateMap<UpdateRefundDto, Refund>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // AssignmentBalanceSummary mappings
        CreateMap<AssignmentBalanceSummary, AssignmentBalanceSummaryResponseDto>()
            .ForMember(dest => dest.BalanceStatus, opt => opt.MapFrom(src => src.BalanceStatus.ToString()))
            .ForMember(dest => dest.RecommendedAction, opt => opt.MapFrom(src => src.RecommendedAction.ToString()));
    }
}
