using AutoMapper;
using QaliTrack.DataManager.Core.Modules.Orders.DTOs;
using QaliTrack.DataManager.Core.Modules.Orders.Entities;
using QaliTrack.DataManager.Core.Modules.Operations.DTOs;
using QaliTrack.DataManager.Core.Modules.Operations.Entities;
using QaliTrack.DataManager.Core.Modules.Quality.DTOs;
using QaliTrack.DataManager.Core.Modules.Quality.Entities;
using QaliTrack.DataManager.Core.Modules.WeightData.DTOs;
using QaliTrack.DataManager.Core.Modules.WeightData.Entities;
using QaliTrack.DataManager.Core.Modules.Transactions.DTOs;
using QaliTrack.DataManager.Core.Modules.Transactions.Entities;

namespace QaliTrack.DataManager.Api.Profiles;

public class DataManagerProfile : Profile
{
    public DataManagerProfile()
    {
        // Order mappings
        CreateMap<CustomerOrder, CustomerOrderDto>();
        CreateMap<CreateCustomerOrderDto, CustomerOrder>();
        CreateMap<UpdateCustomerOrderDto, CustomerOrder>();
        
        CreateMap<CustomerOrderLine, CustomerOrderLineDto>();
        CreateMap<CreateCustomerOrderLineDto, CustomerOrderLine>();
        CreateMap<UpdateCustomerOrderLineDto, CustomerOrderLine>();
        
        CreateMap<PurchaseOrder, PurchaseOrderDto>();
        CreateMap<CreatePurchaseOrderDto, PurchaseOrder>();
        CreateMap<UpdatePurchaseOrderDto, PurchaseOrder>();
        
        CreateMap<PurchaseOrderLine, PurchaseOrderLineDto>();
        CreateMap<CreatePurchaseOrderLineDto, PurchaseOrderLine>();
        CreateMap<UpdatePurchaseOrderLineDto, PurchaseOrderLine>();
        
        CreateMap<InterPlantTransfer, InterPlantTransferDto>();
        CreateMap<CreateInterPlantTransferDto, InterPlantTransfer>();
        CreateMap<UpdateInterPlantTransferDto, InterPlantTransfer>();

        // Operation mappings (if entities exist)
        // Note: Will add specific mappings once entities are confirmed
        
        // Quality mappings (if entities exist)
        // Note: Will add specific mappings once entities are confirmed
        
        // Weight data mappings
        CreateMap<WeightMeasurement, WeightMeasurementDto>().ReverseMap();
        
        // Transaction mappings  
        CreateMap<WeighingTransaction, WeighingTransactionDto>().ReverseMap();
    }
}