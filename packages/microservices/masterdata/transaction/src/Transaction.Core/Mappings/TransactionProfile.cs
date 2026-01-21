using AutoMapper;
using Transaction.Core.DTOs;
using Transaction.Core.Entities;

namespace Transaction.Core.Mappings;

public class TransactionProfile : Profile
{
    public TransactionProfile()
    {
        // WeighbridgeTransaction to TransactionReadDto
        CreateMap<WeighbridgeTransaction, TransactionReadDto>()
            .ReverseMap();

        // CreateTransactionDto to WeighbridgeTransaction
        CreateMap<CreateTransactionDto, WeighbridgeTransaction>()
            .ForMember(dest => dest.TicketID, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "Active"))
            .ForMember(dest => dest.FirstWeightDate, opt => opt.MapFrom(_ => DateTime.Now))
            .ForMember(dest => dest.SecondWeightDate, opt => opt.MapFrom(_ => DateTime.Now))
            .ForMember(dest => dest.SecondWeight, opt => opt.Ignore())
            .ForMember(dest => dest.NetWeight, opt => opt.Ignore())
            .ForMember(dest => dest.ChangeDate, opt => opt.Ignore())
            .ForMember(dest => dest.ChangeDesc, opt => opt.Ignore())
            .ForMember(dest => dest.ReweighPermission, opt => opt.Ignore())
            .ForMember(dest => dest.ApiId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

        // UpdateTransactionDto to WeighbridgeTransaction
        CreateMap<UpdateTransactionDto, WeighbridgeTransaction>()
            .ForMember(dest => dest.TicketID, opt => opt.Ignore())
            .ForMember(dest => dest.ReceiptNo, opt => opt.Ignore())
            .ForMember(dest => dest.FirstWeight, opt => opt.Ignore())
            .ForMember(dest => dest.FirstWeightDate, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.ApiId, opt => opt.Ignore())
            .ForMember(dest => dest.NetWeight, opt => opt.Ignore())
            .ForMember(dest => dest.ChangeDate, opt => opt.Ignore())
            .ForMember(dest => dest.ReweighPermission, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // ReweighRecord mappings (if still needed)
        CreateMap<ReweighRecord, ReweighRecordDto>().ReverseMap()
            .ForMember(dest => dest.WeighbridgeTransaction, opt => opt.Ignore())
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
