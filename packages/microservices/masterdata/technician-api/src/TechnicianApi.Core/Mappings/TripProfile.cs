using AutoMapper;
using TechnicianApi.Core.DTOs.Trip;
using TechnicianApi.Core.Entities;

namespace TechnicianApi.Core.Mappings;

public class TripProfile : Profile
{
    public TripProfile()
    {
        // TripType mappings
        CreateMap<TripType, TripTypeResponseDto>()
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.ToString()))
            .ForMember(dest => dest.EmptyTripOption, opt => opt.MapFrom(src => src.EmptyTripOption.ToString()))
            .ForMember(dest => dest.MaterialRequirement, opt => opt.MapFrom(src => src.MaterialRequirement.ToString()));
        CreateMap<CreateTripTypeDto, TripType>()
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => Enum.Parse<TripCategory>(src.Category)))
            .ForMember(dest => dest.EmptyTripOption, opt => opt.MapFrom(src => Enum.Parse<EmptyTripOption>(src.EmptyTripOption)))
            .ForMember(dest => dest.MaterialRequirement, opt => opt.MapFrom(src => Enum.Parse<MaterialRequirement>(src.MaterialRequirement)))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.Trips, opt => opt.Ignore());

        // Trip mappings
        CreateMap<Trip, TripResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        CreateMap<CreateTripDto, Trip>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => TripStatus.Pending))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Truck, opt => opt.Ignore())
            .ForMember(dest => dest.TripType, opt => opt.Ignore())
            .ForMember(dest => dest.Material, opt => opt.Ignore())
            .ForMember(dest => dest.MaterialVariant, opt => opt.Ignore())
            .ForMember(dest => dest.Expenses, opt => opt.Ignore())
            .ForMember(dest => dest.TripMaterials, opt => opt.Ignore())
            .ForMember(dest => dest.TotalCost, opt => opt.Ignore())
            .ForMember(dest => dest.StartMileage, opt => opt.Ignore())
            .ForMember(dest => dest.EndMileage, opt => opt.Ignore())
            .ForMember(dest => dest.TotalMileage, opt => opt.Ignore())
            .ForMember(dest => dest.ProofImageUrl, opt => opt.Ignore())
            .ForMember(dest => dest.ProofEndImageUrl, opt => opt.Ignore())
            .ForMember(dest => dest.MaterialLoadingPhotosJson, opt => opt.Ignore())
            .ForMember(dest => dest.TruckLocationLatitude, opt => opt.Ignore())
            .ForMember(dest => dest.TruckLocationLongitude, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentLocationLatitude, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentLocationLongitude, opt => opt.Ignore());
        CreateMap<UpdateTripDto, Trip>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // TripMaterial mappings
        CreateMap<TripMaterial, TripMaterialResponseDto>();
        CreateMap<CreateTripMaterialDto, TripMaterial>()
            .ForMember(dest => dest.TotalCost, opt => opt.MapFrom(src => (src.Quantity ?? 1) * src.UnitCost))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.AddedAt, opt => opt.Ignore())
            .ForMember(dest => dest.AddedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.Trip, opt => opt.Ignore())
            .ForMember(dest => dest.Material, opt => opt.Ignore())
            .ForMember(dest => dest.MaterialVariant, opt => opt.Ignore());

        // Expense mappings
        CreateMap<Expense, ExpenseResponseDto>();
        CreateMap<CreateExpenseDto, Expense>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.Synced, opt => opt.Ignore())
            .ForMember(dest => dest.Trip, opt => opt.Ignore())
            .ForMember(dest => dest.Receipts, opt => opt.Ignore());

        // Receipt mappings
        CreateMap<Receipt, ReceiptResponseDto>();
        CreateMap<CreateReceiptDto, Receipt>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.Synced, opt => opt.Ignore())
            .ForMember(dest => dest.ReceiptDetailsJson, opt => opt.Ignore())
            .ForMember(dest => dest.Expense, opt => opt.Ignore());
    }
}
