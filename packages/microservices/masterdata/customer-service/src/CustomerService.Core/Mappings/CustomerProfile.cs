using AutoMapper;
using CustomerService.Core.DTOs;
using CustomerService.Core.Entities;

namespace CustomerService.Core.Mappings;

public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        // Customer mappings
        CreateMap<CustomerService.Core.Entities.Customer, CustomerReadDto>()
            .ForMember(dest => dest.CustomerType, opt => opt.MapFrom(src => src.CustomerType.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        CreateMap<CreateCustomerDto, CustomerService.Core.Entities.Customer>()
            .ForMember(dest => dest.CustomerType, opt => opt.MapFrom(src => Enum.Parse<CustomerType>(src.CustomerType)))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid().ToString()));
        CreateMap<UpdateCustomerDto, CustomerService.Core.Entities.Customer>()
            .ForMember(dest => dest.CustomerType, opt => opt.MapFrom(src => Enum.Parse<CustomerType>(src.CustomerType)));

        // Order mappings
        CreateMap<Order, OrderReadDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.OrderType, opt => opt.MapFrom(src => src.OrderType.ToString()));
        CreateMap<CreateOrderDto, Order>()
            .ForMember(dest => dest.OrderType, opt => opt.MapFrom(src => Enum.Parse<OrderType>(src.OrderType)))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid().ToString()));
        CreateMap<UpdateOrderDto, Order>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // OrderStatusHistory mappings
        CreateMap<OrderStatusHistory, OrderStatusHistoryReadDto>()
            .ForMember(dest => dest.FromStatus, opt => opt.MapFrom(src => src.FromStatus.ToString()))
            .ForMember(dest => dest.ToStatus, opt => opt.MapFrom(src => src.ToStatus.ToString()));

        // Contact mappings
        CreateMap<Contact, ContactReadDto>()
            .ForMember(dest => dest.ContactType, opt => opt.MapFrom(src => src.ContactType.ToString()))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));
        CreateMap<CreateContactDto, Contact>()
            .ForMember(dest => dest.ContactType, opt => opt.MapFrom(src => Enum.Parse<ContactType>(src.ContactType)))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => Enum.Parse<ContactRole>(src.Role)))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid().ToString()));
        CreateMap<UpdateContactDto, Contact>()
            .ForMember(dest => dest.ContactType, opt => opt.MapFrom(src => Enum.Parse<ContactType>(src.ContactType)))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => Enum.Parse<ContactRole>(src.Role)))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<UpdateContactPreferencesDto, Contact>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // ContactCommunication mappings
        CreateMap<ContactCommunication, ContactCommunicationReadDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => src.Direction.ToString()));
        CreateMap<LogCommunicationDto, ContactCommunication>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<CommunicationType>(src.Type)))
            .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => Enum.Parse<CommunicationDirection>(src.Direction)))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid().ToString()));

        // Contract mappings
        CreateMap<Contract, ContractReadDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ContractType, opt => opt.MapFrom(src => src.ContractType.ToString()));
        CreateMap<CreateContractDto, Contract>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ContractStatus.Draft))
            .ForMember(dest => dest.ContractType, opt => opt.MapFrom(src => Enum.Parse<ContractType>(src.ContractType)))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid().ToString()));
        CreateMap<UpdateContractDto, Contract>()
            .ForMember(dest => dest.ContractType, opt => opt.MapFrom(src => Enum.Parse<ContractType>(src.ContractType)))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // ContractRenewal mappings
        CreateMap<ContractRenewal, ContractRenewalReadDto>();
        CreateMap<RenewContractDto, ContractRenewal>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid().ToString()));
    }
}