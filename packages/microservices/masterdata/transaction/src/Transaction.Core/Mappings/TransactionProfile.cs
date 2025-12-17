using AutoMapper;
using Transaction.Core.DTOs;
using Transaction.Core.Entities;

namespace Transaction.Core.Mappings;

public class TransactionProfile : Profile
{
    public TransactionProfile()
    {
        // Base entity mappings - Removed to avoid circular references
        // CreateMap<BaseEntity, object>().IncludeAllDerived();

        // WeighingRecord mappings
        CreateMap<WeighingRecord, WeighingRecordDto>()
            .ReverseMap()
            .ForMember(dest => dest.Transaction, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

        // Audit Log mappings
        CreateMap<TransactionAuditLog, AuditLogDto>().ReverseMap()
            .ForMember(dest => dest.Transaction, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

        // ReweighRecord mappings
        CreateMap<ReweighRecord, ReweighRecordDto>().ReverseMap()
            .ForMember(dest => dest.WeighbridgeTransaction, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

        // WeighbridgeTransaction to TransactionReadDto
        CreateMap<WeighbridgeTransaction, TransactionReadDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => src.IsCompleted))
            .ForMember(dest => dest.CompletedWeighings, opt => opt.MapFrom(src => 
                src.WeighingRecords != null ? src.WeighingRecords.Count : 0))
            .ForMember(dest => dest.ExpectedWeighings, opt => opt.MapFrom(src => src.ExpectedWeighings))
            .ForMember(dest => dest.WeighingRecords, opt => opt.MapFrom(src => 
                src.WeighingRecords != null ? 
                    src.WeighingRecords.OrderBy(w => w.WeighingSequence) : 
                    Enumerable.Empty<WeighingRecord>()))
            .ForMember(dest => dest.CompletedDate, opt => opt.MapFrom(src => src.CompletedDate));

        // CreateTransactionDto to WeighbridgeTransaction
        CreateMap<CreateTransactionDto, WeighbridgeTransaction>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => WeighbridgeTransactionStatus.Pending))
            .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(_ => false))
            .ForMember(dest => dest.ReweighPermissionGranted, opt => opt.MapFrom(_ => false))
            .ForMember(dest => dest.WeighingRecords, opt => opt.Ignore())
            .ForMember(dest => dest.AuditLogs, opt => opt.Ignore())
            .ForMember(dest => dest.ReweighRecords, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.NetWeightCalculatedTimestamp, opt => opt.Ignore())
            .ForMember(dest => dest.CompletedDate, opt => opt.Ignore())
            .ForMember(dest => dest.ChangeDescription, opt => opt.Ignore())
            .ForMember(dest => dest.ChangeDate, opt => opt.Ignore());

        // UpdateTransactionDto to WeighbridgeTransaction
        CreateMap<UpdateTransactionDto, WeighbridgeTransaction>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.IsCompleted, opt => opt.Ignore())
            .ForMember(dest => dest.CompletedDate, opt => opt.Ignore())
            .ForMember(dest => dest.ReweighPermissionGranted, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.WeighingRecords, opt => opt.Ignore())
            .ForMember(dest => dest.AuditLogs, opt => opt.Ignore())
            .ForMember(dest => dest.ReweighRecords, opt => opt.Ignore())
            .ForMember(dest => dest.NetWeightCalculatedTimestamp, opt => opt.Ignore())
            .ForMember(dest => dest.ChangeDate, opt => opt.Ignore());

        // AddWeighingDto to WeighingRecord
        CreateMap<AddWeighingDto, WeighingRecord>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.WeighbridgeTransactionId, opt => opt.MapFrom(src => src.TransactionId))
            .ForMember(dest => dest.WeighingDate, opt => opt.Ignore())  // Will be set in service
            .ForMember(dest => dest.WeighingSequence, opt => opt.Ignore())  // Will be set in service
            .ForMember(dest => dest.Transaction, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

        // RequestReweighDto to WeighbridgeTransaction
        CreateMap<RequestReweighDto, WeighbridgeTransaction>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ReweighReason, opt => opt.MapFrom(src => src.Reason))
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());  // Will be set in service

        // StartReweighDto to ReweighRecord
        CreateMap<StartReweighDto, ReweighRecord>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.WeighbridgeTransactionId, opt => opt.Ignore())
            .ForMember(dest => dest.AttemptNumber, opt => opt.Ignore())
            .ForMember(dest => dest.StartedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CompletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.Weight1, opt => opt.Ignore())
            .ForMember(dest => dest.Weight2, opt => opt.Ignore())
            .ForMember(dest => dest.NetWeight, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.WeighbridgeTransaction, opt => opt.Ignore());

        // AddReweighWeightDto to ReweighRecord
        CreateMap<AddReweighWeightDto, ReweighRecord>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.WeighbridgeTransactionId, opt => opt.Ignore())
            .ForMember(dest => dest.AttemptNumber, opt => opt.Ignore())
            .ForMember(dest => dest.StartedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CompletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.Weight1, opt => opt.Ignore())
            .ForMember(dest => dest.Weight2, opt => opt.Ignore())
            .ForMember(dest => dest.NetWeight, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.WeighbridgeTransaction, opt => opt.Ignore());
        
        CreateMap<WeighbridgeTransaction, TransactionReadDto>()
            .ForMember(dest => dest.NprImageUrl, opt => opt.MapFrom(src => src.NPR))
            .ForMember(dest => dest.TransactionImageUrl, opt => opt.MapFrom(src => src.Image));
        // Update the CreateTransactionDto to WeighbridgeTransaction mapping
        CreateMap<CreateTransactionDto, WeighbridgeTransaction>()
            // ... existing mappings ...
            // Add these lines to include the missing properties
            .ForMember(dest => dest.CommodityId, opt => opt.MapFrom(src => src.CommodityId))
            .ForMember(dest => dest.CommodityName, opt => opt.MapFrom(src => src.CommodityName))
            .ForMember(dest => dest.SupplierId, opt => opt.MapFrom(src => src.SupplierId))
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.SupplierName))
            .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.CustomerName))
            .ForMember(dest => dest.OriginId, opt => opt.MapFrom(src => src.OriginId))
            .ForMember(dest => dest.OriginName, opt => opt.MapFrom(src => src.OriginName))
            .ForMember(dest => dest.DestinationId, opt => opt.MapFrom(src => src.DestinationId))
            .ForMember(dest => dest.DestinationName, opt => opt.MapFrom(src => src.DestinationName)).ForMember(dest => dest.WeighMode, opt => opt.MapFrom(src => src.WeighMode));

        // ... rest of the existing mappings ...
        // DTO to Entity (excluding image files as they're handled separately)
        CreateMap<CreateTransactionDto, WeighbridgeTransaction>()
            .ForMember(dest => dest.NPR, opt => opt.Ignore())
            .ForMember(dest => dest.Image, opt => opt.Ignore());
    }
}