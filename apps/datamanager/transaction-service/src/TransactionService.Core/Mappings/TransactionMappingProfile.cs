using AutoMapper;
using TransactionService.Core.DTOs;
using TransactionService.Core.Entities;

namespace TransactionService.Core.Mappings;

public class TransactionMappingProfile : Profile
{
    public TransactionMappingProfile()
    {
        // Transaction mappings
        CreateMap<WeighingTransaction, TransactionDto>()
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => src.Metadata));
        
        CreateMap<CreateTransactionRequest, WeighingTransaction>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TransactionNumber, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => TransactionStatus.Pending))
            .ForMember(dest => dest.TransactionDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.WorkflowSteps, opt => opt.Ignore())
            .ForMember(dest => dest.Charges, opt => opt.Ignore())
            .ForMember(dest => dest.Documents, opt => opt.Ignore())
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => src.Metadata));

        // Workflow mappings
        CreateMap<TransactionWorkflow, TransactionWorkflowDto>()
            .ForMember(dest => dest.ValidationData, opt => opt.MapFrom(src => src.ValidationData));

        // Charge mappings
        CreateMap<TransactionCharge, TransactionChargeDto>();
        CreateMap<CreateChargeRequest, TransactionCharge>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TaxAmount, opt => opt.Ignore())
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
            .ForMember(dest => dest.IsApproved, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.IsPaid, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Transaction, opt => opt.Ignore());

        // Document mappings
        CreateMap<TransactionDocument, TransactionDocumentDto>();
    }
}