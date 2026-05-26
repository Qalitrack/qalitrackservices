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
            .ForMember(dest => dest.SecondWeightDate, opt => opt.Ignore())
            .ForMember(dest => dest.SecondWeight, opt => opt.Ignore())
            .ForMember(dest => dest.NetWeight, opt => opt.Ignore())
            .ForMember(dest => dest.TurnaroundTime, opt => opt.Ignore())
            .ForMember(dest => dest.ChangeDate, opt => opt.Ignore())
            .ForMember(dest => dest.ChangeDesc, opt => opt.Ignore())
            .ForMember(dest => dest.ReweighPermission, opt => opt.Ignore())
            .ForMember(dest => dest.ApiId, opt => opt.Ignore())
            .ForMember(dest => dest.IsReweighed, opt => opt.MapFrom(_ => false))
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
            .ForMember(dest => dest.TurnaroundTime, opt => opt.Ignore())
            .ForMember(dest => dest.ChangeDate, opt => opt.Ignore())
            .ForMember(dest => dest.ReweighPermission, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // ReweighRecord mappings (if still needed)
        CreateMap<ReweighRecord, ReweighRecordDto>().ReverseMap()
            .ForMember(dest => dest.WeighbridgeTransaction, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
    }
}