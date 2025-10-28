using AutoMapper;
using Transaction.Core.DTOs;
using Transaction.Core.Entities;

namespace Transaction.Core.Mappings;

public class TransactionProfile : Profile
{
    public TransactionProfile()
    {
        // Entity to DTO mappings
        CreateMap<WeighbridgeTransaction, TransactionReadDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ReweighPermissionGranted, opt => opt.MapFrom(src => src.ReweighPermissionGranted))
            .ForMember(dest => dest.ReweighPermissionReason, opt => opt.MapFrom(src => src.ReweighPermissionReason));

        CreateMap<WeighingRecord, WeighingRecordDto>();
        CreateMap<TransactionAuditLog, AuditLogDto>();

        // DTO to Entity mappings
        CreateMap<CreateTransactionDto, WeighbridgeTransaction>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => WeighbridgeTransactionStatus.Pending))
            .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.CompletedWeighings, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.FirstWeightTimestamp, opt => opt.Ignore())
            .ForMember(dest => dest.SecondWeightTimestamp, opt => opt.Ignore())
            .ForMember(dest => dest.NetWeightCalculatedTimestamp, opt => opt.Ignore())
            .ForMember(dest => dest.CompletedDate, opt => opt.Ignore())
            .ForMember(dest => dest.SecondWeight, opt => opt.Ignore())
            .ForMember(dest => dest.NetWeight, opt => opt.Ignore())
            .ForMember(dest => dest.WeighBridgeName2nd, opt => opt.Ignore())
            .ForMember(dest => dest.ScaleName2nd, opt => opt.Ignore())
            .ForMember(dest => dest.OperatorId2nd, opt => opt.Ignore())
            .ForMember(dest => dest.OperatorName2nd, opt => opt.Ignore())
            .ForMember(dest => dest.ChangeDescription, opt => opt.Ignore())
            .ForMember(dest => dest.ChangeDate, opt => opt.Ignore())
            .ForMember(dest => dest.WeighingRecords, opt => opt.Ignore())
            .ForMember(dest => dest.AuditLogs, opt => opt.Ignore())
            .ForMember(dest => dest.ReweighPermissionGranted, opt => opt.MapFrom(src => false));

        // UpdateDto to Entity - only map provided fields
        CreateMap<UpdateTransactionDto, WeighbridgeTransaction>()
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.IsCompleted, opt => opt.Ignore())
            .ForMember(dest => dest.CompletedDate, opt => opt.Ignore())
            .ForMember(dest => dest.ReweighPermissionGranted, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.FirstWeightTimestamp, opt => opt.Ignore())
            .ForMember(dest => dest.SecondWeightTimestamp, opt => opt.Ignore())
            .ForMember(dest => dest.NetWeightCalculatedTimestamp, opt => opt.Ignore())
            .ForMember(dest => dest.ChangeDate, opt => opt.Ignore())
            .ForMember(dest => dest.WeighingRecords, opt => opt.Ignore())
            .ForMember(dest => dest.AuditLogs, opt => opt.Ignore());

        CreateMap<AddWeighingDto, WeighingRecord>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.WeighbridgeTransactionId, opt => opt.MapFrom(src => src.TransactionId))
            .ForMember(dest => dest.Weight, opt => opt.MapFrom(src => src.Weight))
            .ForMember(dest => dest.WeighingDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Transaction, opt => opt.Ignore())
            .ForMember(dest => dest.WeighingSequence, opt => opt.Ignore());
            
        // DTO for reweigh requests
        // DTO for reweigh requests
        CreateMap<RequestReweighDto, WeighbridgeTransaction>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ReweighPermissionReason, opt => opt.MapFrom(src => src.Reason))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
    }
}