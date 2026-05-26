using System.Text.Json;
using AutoMapper;
using Masterdata.Core.DTOs.Customer;
using Masterdata.Core.DTOs.Drivers;
using Masterdata.Core.DTOs.Product;
using Masterdata.Core.DTOs.Route;
using Masterdata.Core.DTOs.Supplier;
using Masterdata.Core.DTOs.Transporters;
using Masterdata.Core.DTOs.Vehicles;
using Masterdata.Core.DTOs.Owner;
using Masterdata.Core.DTOs.Weighbridge;
using Masterdata.Core.DTOs.Organisation;
using Masterdata.Core.DTOs.Affiliation;
using Masterdata.Core.DTOs.AuditLog;
using Masterdata.Core.DTOs.Axle;
using Masterdata.Core.DTOs.Sacco;
using Masterdata.Core.Entities;
using Masterdata.Core.Models;

namespace Masterdata.Core.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Driver mappings
        CreateMap<CreateDriverDto, Driver>();
        CreateMap<UpdateDriverDto, Driver>();
        CreateMap<Driver, DriverReadDto>();

        // Transporter mappings
        CreateMap<CreateTransporterDto, Transporter>();
        CreateMap<UpdateTransporterDto, Transporter>();
        CreateMap<Transporter, TransporterReadDto>();

        // Vehicle mappings
        CreateMap<CreateVehicleDto, Vehicle>();
        CreateMap<UpdateVehicleDto, Vehicle>();
        CreateMap<Vehicle, VehicleReadDto>();
        
        // Supplier mappings
        CreateMap<CreateSupplierDto, Supplier>()
            .ForMember(dest => dest.ContactInfo, opt => opt.MapFrom((src, dest) =>
                JsonSerializer.Serialize(new
                {
                    ContactPerson = src.ContactPerson,
                    Email = src.Email,
                    Phone = src.Phone,
                    Address = src.Address,
                    City = src.City,
                    Country = src.Country,
                    TaxNumber = src.TaxNumber,
                    RegistrationNumber = src.RegistrationNumber
                })));
        CreateMap<UpdateSupplierDto, Supplier>()
            .ForMember(dest => dest.ContactInfo, opt => opt.MapFrom((src, dest) =>
                JsonSerializer.Serialize(new
                {
                    ContactPerson = src.ContactPerson,
                    Email = src.Email,
                    Phone = src.Phone,
                    Address = src.Address,
                    City = src.City,
                    Country = src.Country,
                    TaxNumber = src.TaxNumber,
                    RegistrationNumber = src.RegistrationNumber
                })));
        CreateMap<Supplier, SupplierReadDto>()
            .ForMember(dest => dest.ContactPerson, opt => opt.MapFrom((src, dest) =>
                GetContactField(src.ContactInfo, "ContactPerson")))
            .ForMember(dest => dest.Email, opt => opt.MapFrom((src, dest) =>
                GetContactField(src.ContactInfo, "Email")))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom((src, dest) =>
                GetContactField(src.ContactInfo, "Phone")))
            .ForMember(dest => dest.Address, opt => opt.MapFrom((src, dest) =>
                GetContactField(src.ContactInfo, "Address")))
            .ForMember(dest => dest.City, opt => opt.MapFrom((src, dest) =>
                GetContactField(src.ContactInfo, "City")))
            .ForMember(dest => dest.Country, opt => opt.MapFrom((src, dest) =>
                GetContactField(src.ContactInfo, "Country")))
            .ForMember(dest => dest.TaxNumber, opt => opt.MapFrom((src, dest) =>
                GetContactField(src.ContactInfo, "TaxNumber")))
            .ForMember(dest => dest.RegistrationNumber, opt => opt.MapFrom((src, dest) =>
                GetContactField(src.ContactInfo, "RegistrationNumber")));
        
        // Product mappings
        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();
        CreateMap<Product, ProductDto>();
        
        // Route mappings
        CreateMap<CreateRouteDto, Route>();
        CreateMap<UpdateRouteDto, Route>();
        CreateMap<Route, RouteDto>();
        
        // Customer mappings
        CreateMap<CreateCustomerDto, Customer>();
        CreateMap<UpdateCustomerDto, Customer>();
        CreateMap<Customer, CustomerDto>();
        
        // Weighbridge mappings
        CreateMap<CreateWeighbridgeDto, Weighbridge>();
        CreateMap<UpdateWeighbridgeDto, Weighbridge>();
        CreateMap<Weighbridge, WeighbridgeDto>();
        
        // Owner mappings
        CreateMap<CreateOwnerDto, Owner>();
        CreateMap<UpdateOwnerDto, Owner>();
        CreateMap<Owner, OwnerDto>()
            .ForMember(dest => dest.ContactInfo, opt => 
                opt.MapFrom((src, dest) => 
                    !string.IsNullOrEmpty(src.ContactInfo) ? 
                        JsonDocument.Parse(src.ContactInfo) : 
                        null));
        
        // Organisation mappings
        CreateMap<CreateOrganisationDto, Organisation>();
        CreateMap<UpdateOrganisationDto, Organisation>();
        CreateMap<Organisation, OrganisationDto>()
            .ForMember(dest => dest.ContactInfo, opt => 
                opt.MapFrom((src, dest) => 
                    !string.IsNullOrEmpty(src.ContactInfo) ? 
                        JsonDocument.Parse(src.ContactInfo) : 
                        null));
        
        // Affiliation mappings
        CreateMap<CreateAffiliationDto, Affiliation>();
        CreateMap<Affiliation, AffiliationDto>()
            .ForMember(dest => dest.SaccoName, opt => 
                opt.MapFrom(src => src.Sacco != null ? src.Sacco.Name : null))
            .ForMember(dest => dest.OrganisationName, opt => 
                opt.MapFrom(src => src.Organisation != null ? src.Organisation.Name : null));
        
        // Sacco mappings
        CreateMap<CreateSaccoDto, Sacco>();
        CreateMap<UpdateSaccoDto, Sacco>();
        CreateMap<Sacco, SaccoDto>();
        
        
        // AuditLog mappings
        CreateMap<AuditLog, AuditLogDto>();
        
        CreateMap<AuditLog, AuditLogReadDto>()
            .ForMember(dest => dest.OldValues, opt => opt.MapFrom((src, dest) => 
                !string.IsNullOrEmpty(src.OldValues) ? JsonDocument.Parse(src.OldValues) : null))
            .ForMember(dest => dest.NewValues, opt => opt.MapFrom((src, dest) => 
                !string.IsNullOrEmpty(src.NewValues) ? JsonDocument.Parse(src.NewValues) : null))
            .ForMember(dest => dest.AffectedProperties, opt => opt.MapFrom(src => 
                !string.IsNullOrEmpty(src.AffectedProperties) ? src.AffectedProperties.Split(',', StringSplitOptions.RemoveEmptyEntries) : null));
        
        CreateMap<CreateAuditLogDto, AuditLog>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.OldValues, opt => opt.MapFrom((src, dest, member, context) => 
                src.OldValues != null ? JsonSerializer.Serialize(src.OldValues.RootElement) : null))
            .ForMember(dest => dest.NewValues, opt => opt.MapFrom((src, dest, member, context) => 
                src.NewValues != null ? JsonSerializer.Serialize(src.NewValues.RootElement) : null))
            .ForMember(dest => dest.AffectedProperties, opt => opt.MapFrom(src => 
                src.AffectedProperties != null ? string.Join(", ", src.AffectedProperties) : null));

        // AxleConfiguration mappings
        CreateMap<AxleConfiguration, AxleConfigurationDto>();
        CreateMap<CreateAxleConfigurationDto, AxleConfiguration>();
        CreateMap<UpdateAxleConfigurationDto, AxleConfiguration>();

        // AuditLogFilterDto to AuditLogFilterParams mapping
        CreateMap<AuditLogFilterDto, AuditLogFilterParams>();
    }

    private static string? GetContactField(string? json, string fieldName)
    {
        if (string.IsNullOrEmpty(json)) return null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (string.Equals(prop.Name, fieldName, StringComparison.OrdinalIgnoreCase))
                    return prop.Value.ValueKind == JsonValueKind.String ? prop.Value.GetString() : null;
            }
        }
        catch { }
        return null;
    }
}