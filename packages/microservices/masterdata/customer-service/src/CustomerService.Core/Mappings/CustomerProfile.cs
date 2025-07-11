using AutoMapper;
using CustomerService.Core.DTOs;
using CustomerService.Core.Entities;

namespace CustomerService.Core.Mappings;

public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        CreateMap<Customer, CustomerDto>();
        CreateMap<Customer, CustomerDetailDto>()
            .ForMember(dest => dest.Contacts, opt => opt.MapFrom(src => src.Contacts))
            .ForMember(dest => dest.Contracts, opt => opt.MapFrom(src => src.Contracts))
            .ForMember(dest => dest.Locations, opt => opt.MapFrom(src => src.Locations))
            .ForMember(dest => dest.Billing, opt => opt.MapFrom(src => src.Billing))
            .ForMember(dest => dest.Credit, opt => opt.MapFrom(src => src.Credit))
            .ForMember(dest => dest.Preferences, opt => opt.MapFrom(src => src.Preferences));

        CreateMap<RegisterCustomerRequest, Customer>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => CustomerStatus.Active))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Contacts, opt => opt.Ignore())
            .ForMember(dest => dest.Contracts, opt => opt.Ignore())
            .ForMember(dest => dest.Locations, opt => opt.Ignore())
            .ForMember(dest => dest.Documents, opt => opt.Ignore())
            .ForMember(dest => dest.Billing, opt => opt.Ignore())
            .ForMember(dest => dest.Credit, opt => opt.Ignore())
            .ForMember(dest => dest.Preferences, opt => opt.Ignore());

        CreateMap<UpdateCustomerRequest, Customer>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Contacts, opt => opt.Ignore())
            .ForMember(dest => dest.Contracts, opt => opt.Ignore())
            .ForMember(dest => dest.Locations, opt => opt.Ignore())
            .ForMember(dest => dest.Documents, opt => opt.Ignore())
            .ForMember(dest => dest.Billing, opt => opt.Ignore())
            .ForMember(dest => dest.Credit, opt => opt.Ignore())
            .ForMember(dest => dest.Preferences, opt => opt.Ignore());

        CreateMap<CustomerContact, CustomerContactDto>();
        CreateMap<CreateCustomerContactRequest, CustomerContact>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        CreateMap<UpdateCustomerContactRequest, CustomerContact>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Customer, opt => opt.Ignore());

        CreateMap<CustomerContract, CustomerContractDto>();
        CreateMap<CreateCustomerContractRequest, CustomerContract>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ContractStatus.Draft))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.SignedByCustomer, opt => opt.Ignore())
            .ForMember(dest => dest.SignedByCompany, opt => opt.Ignore())
            .ForMember(dest => dest.SignedDate, opt => opt.Ignore());

        CreateMap<UpdateCustomerContractRequest, CustomerContract>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Customer, opt => opt.Ignore());

        CreateMap<CustomerBilling, CustomerBillingDto>();
        CreateMap<UpdateCustomerBillingRequest, CustomerBilling>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Customer, opt => opt.Ignore());

        CreateMap<CustomerCredit, CustomerCreditDto>();
        CreateMap<PaymentHistory, PaymentHistoryDto>();
        CreateMap<UpdateCustomerCreditRequest, CustomerCredit>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentHistory, opt => opt.Ignore())
            .ForMember(dest => dest.AvailableCredit, opt => opt.Ignore())
            .ForMember(dest => dest.UsedCredit, opt => opt.Ignore())
            .ForMember(dest => dest.LastCreditCheck, opt => opt.Ignore());

        CreateMap<CustomerLocation, CustomerLocationDto>();
        CreateMap<CustomerPreference, CustomerPreferenceDto>();
        CreateMap<NotificationSettings, NotificationSettingsDto>();
        CreateMap<CommunicationPreferences, CommunicationPreferencesDto>();

        // Order mappings
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : ""))
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : ""))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.OrderType, opt => opt.MapFrom(src => src.OrderType.ToString()));

        CreateMap<Order, OrderSummaryDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : ""))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<OrderStatusHistory, OrderStatusHistoryDto>()
            .ForMember(dest => dest.FromStatus, opt => opt.MapFrom(src => src.FromStatus.ToString()))
            .ForMember(dest => dest.ToStatus, opt => opt.MapFrom(src => src.ToStatus.ToString()));

        CreateMap<CreateOrderDto, Order>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrderNumber, opt => opt.Ignore())
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
            .ForMember(dest => dest.OrderDate, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.Supplier, opt => opt.Ignore())
            .ForMember(dest => dest.StatusHistory, opt => opt.Ignore());
    }
}