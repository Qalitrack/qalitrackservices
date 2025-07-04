using AutoMapper;
using WeightDataService.Core.DTOs;
using WeightDataService.Core.Entities;

namespace WeightDataService.Core.Mappings;

public class WeightCorrectionProfile : Profile
{
    public WeightCorrectionProfile()
    {
        CreateMap<WeightCorrection, WeightCorrectionDto>();

        CreateMap<CreateWeightCorrectionDto, WeightCorrection>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.OriginalWeight, opt => opt.Ignore())
            .ForMember(dest => dest.AuthorizedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CorrectionDateTime, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsApproved, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.ApprovedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovalDateTime, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovalNotes, opt => opt.Ignore())
            .ForMember(dest => dest.WeightMeasurement, opt => opt.Ignore());

        CreateMap<ApproveWeightCorrectionDto, WeightCorrection>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.WeightMeasurementId, opt => opt.Ignore())
            .ForMember(dest => dest.OriginalWeight, opt => opt.Ignore())
            .ForMember(dest => dest.CorrectedWeight, opt => opt.Ignore())
            .ForMember(dest => dest.Reason, opt => opt.Ignore())
            .ForMember(dest => dest.AuthorizedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CorrectionDateTime, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovalDateTime, opt => opt.MapFrom(src => src.IsApproved ? DateTime.UtcNow : (DateTime?)null))
            .ForMember(dest => dest.WeightMeasurement, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}